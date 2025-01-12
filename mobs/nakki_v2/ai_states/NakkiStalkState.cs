using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiStalkState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiIdleState IdleState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_idleState);
		set => this.SetExportProperty(ref _idleState, value, notifyPropertyListChanged: true);
	}
	private NakkiIdleState? _idleState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value, notifyPropertyListChanged: true);
	}
	private NakkiAttackState? _attackState;

	[Export]
	[MustSetInEditor]
	public NakkiUnderwaterState DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value, notifyPropertyListChanged: true);
	}
	private NakkiUnderwaterState? _diveState;


	[ExportGroup("")]
	[Export] public float StalkThreshold { get; set; } = 40.0f;
	[Export] public float StalkTime { get; set; } = 5.0f;
	[Export] public float StalkTimeVariation { get; set; } = 0.5f;
	[Export] public float DiveChance { get; set; } = 0.2f;

	private Timer? _timer;
	private bool _isDoneStalking;
	private RandomNumberGenerator _rng = new();


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneStalking = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			nakki.CurrentState = IdleState;
			return;
		}

		var relative = nakki.GetPlayerXPositionRelative(player);
		nakki.SetProgressTarget(relative);
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneStalking = false;

		_timer!.WaitTime = _rng.RandomWithVariation(StalkTime, StalkTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		if (nakki.DetectionLevel <= 0.0f) {

			var canDive = DiveState!.IsStateReady(nakki);
			if (canDive && _rng.DiceRoll(DiveChance)) {
				nakki.CurrentState = DiveState;
			} else {
				nakki.CurrentState = IdleState;
			}

			return;
		}

		if (nakki.DetectionLevel >= AttackState!.AttackThreshold) {
			nakki.CurrentState = AttackState;
			return;
		}
	}
}
