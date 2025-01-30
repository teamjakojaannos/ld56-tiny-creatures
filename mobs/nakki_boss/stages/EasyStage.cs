using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;
using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class EasyStage : NakkiBossStage {
	[Export] public float TimeBetweenAttacks { get; set; } = 5.0f;
	[Export] public int MinAttacksBeforeNextState { get; set; } = 3;
	[Export] public int MaxAttacksBeforeNextState { get; set; } = 6;

	[ExportGroup("SweepAttack")]
	[Export] public float HandSpeed { get; set; } = 50.0f;
	[Export(PropertyHint.Range, "0,1.0")]
	public float DoSweepOnTopOfPlayerChance { get; set; } = 0.50f;

	[ExportGroup("LilypadAttack")]
	[Export] public float UnderwaterTime { get; set; } = 1.5f;
	[Export] public float UnderwaterTimeVariation { get; set; } = 0.5f;
	[Export] public float SinkSpeed { get; set; } = 1.0f;
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
	private int _waitingForAttackIdToFinish = -1;


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

		Attacks[] possibleAttacks = [Attacks.Lilypad, Attacks.Sweep];
		var attackToPlay = _rng.PickRandomUnchecked(possibleAttacks);
		switch (attackToPlay) {
			case Attacks.Lilypad: {
					DoLilypadAttack();
					return;
				}
			case Attacks.Sweep: {
					DoSweepAttack();
					return;
				}
			default: break;
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_attackCount = 0;
		_readyToAttack = true;
		_waitingForAttackIdToFinish = -1;
	}

	public override void ExitState(NakkiV2 nakki) {
		AttackTimer.Stop();
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }

	public override void LilypadAttackWasCompleted(int attackId) {
		if (attackId == _waitingForAttackIdToFinish) {
			StartCooldown();
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

	private void DoLilypadAttack() {
		_readyToAttack = false;
		_attackCount += 1;

		var stats = new LilypadAttackStats(new RandomSelection(5, "stage_1"));
		_waitingForAttackIdToFinish = stats.AttackId;
		EmitSignal(NakkiBossStage.SignalName.LilypadAttackInitiated, stats);
	}

	private void DoSweepAttack() {
		_readyToAttack = false;
		_attackCount += 1;

		var sweepPosition = _rng.DiceRoll(DoSweepOnTopOfPlayerChance)
			? GetClosestPositionToPlayer()
			: SweepAttackPositions.PickRandom();

		var sweep = SweepAttackScene.Instantiate<SweepAttack>();
		sweep.AttackDone += StartCooldown;
		sweep.Speed = HandSpeed;

		/* We need to set position before we add attack node as a child, otherwise we could
			accidentally spawn it on top of player for 1 frame (or 0.1 of a frame), causing it
			to deal damage to player
			Desired outcome: "attack global pos = marker global pos"
			=> attack global pos = attack pos + parent global pos
			   attack pos = attack global pos - parent global pos
			              = marker global pos - parent global pos
		*/
		var position = sweepPosition.GlobalPosition - SweepAttackContainer.GlobalPosition;
		sweep.Position = position;

		SweepAttackContainer.AddChild(sweep);
		sweep.StartAttack();
	}

	private Node2D GetClosestPositionToPlayer() {
		var playerPos = this.Persistent().Player.GlobalPosition;

		var closest = SweepAttackPositions
			.Select(node => {
				var xDistanceFromPlayer = Mathf.Abs(node.GlobalPosition.X - playerPos.X);
				return (xDistanceFromPlayer, node);
			})
			.MinBy(a => a.xDistanceFromPlayer)
			.node;

		return closest;
	}

	private enum Attacks {
		Lilypad,
		Sweep,
	}
}

