using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiDiveOnTriggerState : NakkiAiState {
	[Export] private NakkiUnderwaterState? _diveState;
	[Export] private NakkiStalkState? _stalkState;
	[Export] private NakkiAttackState? _attackState;

	public override void _Ready() {
		if (_diveState == null) {
			GD.PrintErr("Dive state is null");
		}

		if (_stalkState == null) {
			GD.PrintErr("Stalk state is null");
		}

		if (_attackState == null) {
			GD.PrintErr("Attack state is null");
		}
	}

	public override void ReceiveTrigger(NakkiV2 nakki) {
		nakki.SwitchToState(_diveState!);
	}

	public override void AiUpdate(NakkiV2 nakki) { }

	public override void EnterState(NakkiV2 nakki) { }

	public override void ExitState(NakkiV2 nakki) { }

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		if (nakki._detectionLevel >= nakki._attackThreshold) {
			nakki.SwitchToState(_attackState!);
			return;
		}

		if (nakki._detectionLevel >= nakki._stalkThreshold) {
			nakki.SwitchToState(_stalkState!);
			return;
		}
	}
}
