using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiStalkState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] public float _stalkThreshold = 40.0f;
	[Export] private float _stalkTime = 5.0f;
	[Export] private float _stalkTimeVariation = 0.5f;
	[Export] private float _diveChance = 0.2f;

	[Export]
	[MustSetInEditor]
	public NakkiIdleState? IdleState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_idleState);
		set => this.SetExportProperty(ref _idleState, value, notifyPropertyListChanged: true);
	}
	private NakkiIdleState? _idleState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState? AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value, notifyPropertyListChanged: true);
	}
	private NakkiAttackState? _attackState;

	[Export]
	[MustSetInEditor]
	public NakkiUnderwaterState? DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value, notifyPropertyListChanged: true);
	}
	private NakkiUnderwaterState? _diveState;


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
			nakki.SwitchToState(IdleState!);
			return;
		}

		var relative = nakki.GetPlayerXPositionRelative(player);
		nakki.SetProgressTarget(relative);
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneStalking = false;

		_timer!.WaitTime = _rng.RandomWithVariation(_stalkTime, _stalkTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		if (nakki._detectionLevel <= 0.0f) {

			var canDive = DiveState!.IsStateReady(nakki);
			if (canDive && _rng.DiceRoll(_diveChance)) {
				nakki.SwitchToState(DiveState!);
			} else {
				nakki.SwitchToState(IdleState!);
			}

			return;
		}

		if (nakki._detectionLevel >= AttackState!._attackThreshold) {
			nakki.SwitchToState(AttackState!);
			return;
		}
	}
}
