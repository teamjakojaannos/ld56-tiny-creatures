using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class SecondStage : NakkiBossStage {
	[Export] public float TimeBetweenAttacks { get; set; } = 2.0f;
	[Export] public int MinAttacksBeforeNextState { get; set; } = 3;
	[Export] public int MaxAttacksBeforeNextState { get; set; } = 6;


	[ExportGroup("SweepAttack")]
	[Export] public float HandSpeed { get; set; } = 75.0f;

	[ExportGroup("LilypadAttack")]
	[Export] public float UnderwaterTime { get; set; } = 1.5f;
	[Export] public float UnderwaterTimeVariation { get; set; } = 0.5f;
	[Export] public float SinkSpeed { get; set; } = 1.5f;
	[Export] public float SinkSpeedVariation { get; set; } = 0.5f;


	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiAiState NextState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nextState);
		set => this.SetExportProperty(ref _nextState, value);
	}
	private NakkiAiState? _nextState;

	[Export]
	[MustSetInEditor]
	public PackedScene SweepAttackScene {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sweepAttackScene);
		set => this.SetExportProperty(ref _sweepAttackScene, value);
	}
	private PackedScene? _sweepAttackScene;

	[Export]
	[MustSetInEditor]
	public Node2D SweepAttackContainer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sweepAttackContainer);
		set => this.SetExportProperty(ref _sweepAttackContainer, value);
	}
	private Node2D? _sweepAttackContainer;

	[Export] public Array<Node2D> SweepAttackPositions { get; set; } = [];

	private Timer AttackTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackTimer);
		set => this.SetExportProperty(ref _attackTimer, value);
	}
	private Timer? _attackTimer;


	private bool _readyToAttack = true;
	private int _attackCount = 0;
	private RandomNumberGenerator _rng = new();
	private Dictionary<int, LilypadAttackStats> _waves = [];
	private bool _isDoingLilypadAttack = false;


	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		if (SweepAttackPositions.Count == 0) {
			warnings = warnings.Append("List of sweep attack positions is empty!").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		AttackTimer = new Timer {
			Autostart = false,
			OneShot = true,
		};
		AttackTimer.Timeout += () => {
			_readyToAttack = true;
		};
		AddChild(AttackTimer);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (!_readyToAttack) {
			return;
		}

		if (ShouldSwitchState()) {
			nakki.CurrentState = NextState;
			return;
		}

		if (!_isDoingLilypadAttack) {
			DoLilypadWaveAttack();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_attackCount = 0;
		_readyToAttack = true;
		_waves.Clear();
		_isDoingLilypadAttack = false;
	}

	public override void ExitState(NakkiV2 nakki) {
		AttackTimer.Stop();
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		/* If näkki is doing a lilypad-wave-attack, we can start a new attack as soon as
			it finishes the animation for it, we don't need to wait for the lilypads to emerge.
			In other words:
				1) do animation for wave attack
				2) näkki is done with wave animation, lilypads start sinking
				3) näkki can do sweep attacks while wave is going on
		*/
		var isWaveOngoing = _waves.Count > 0;
		if (animation == NakkiAnimation.LilypadAttack && isWaveOngoing) {
			StartCooldown();
		}
	}

	public override void LilypadAttackWasCompleted(int attackId) {
		if (_waves.TryGetValue(attackId, out var nextAttack)) {
			// lilypads from previous wave emerged -> start next wave
			EmitSignal(NakkiBossStage.SignalName.LilypadAttackInitiated, nextAttack);
			_waves.Remove(attackId);
		} else {
			// we either finished the last wave, or a regular lilypad attack
			_isDoingLilypadAttack = false;
		}
	}

	private void StartCooldown() {
		AttackTimer.Stop();
		AttackTimer.Start(TimeBetweenAttacks);
	}

	private bool ShouldSwitchState() {
		var roll = _rng.RandiRange(MinAttacksBeforeNextState, MaxAttacksBeforeNextState);
		return _attackCount >= roll;
	}

	private void DoLilypadWaveAttack() {
		_readyToAttack = false;
		_attackCount += 1;
		_isDoingLilypadAttack = true;

		var id1 = LilypadAttackStats.GenerateId();
		var id2 = LilypadAttackStats.GenerateId();
		var id3 = LilypadAttackStats.GenerateId();

		var firstWave = WaveStats(id1, "col_1", true);
		var secondWave = WaveStats(id2, "col_2", false);
		var thirdWave = WaveStats(id3, "col_3", false);

		_waves.Add(id1, secondWave);
		_waves.Add(id2, thirdWave);

		EmitSignal(NakkiBossStage.SignalName.LilypadAttackInitiated, firstWave);
	}

	private static LilypadAttackStats WaveStats(int id, string tag, bool playNakkiAnimation) {
		return new() {
			UnderwaterTime = 1.5f,
			UnderwaterTimeVariation = 0.1f,
			SinkSpeed = 1.0f,
			SinkSpeedVariation = 0.1f,
			ShakeTime = 0.75f,
			ShakeTimeVariation = 0.1f,
			AttackId = id,
			SelectionStrategy = new SelectByTag(tag),
			PlayNakkiAnimation = playNakkiAnimation,
		};
	}

	private enum Attacks {
		Lilypad,
		Sweep,
	}
}
