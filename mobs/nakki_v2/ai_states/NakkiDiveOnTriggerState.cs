using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiDiveOnTriggerState : NakkiAiState {

	[Export] private string _stateName = "dive_on_trigger";
	[Export] private string _diveStateName = "underwater";

	public override void _Ready() {
	}

	public override string StateName() {
		return _stateName;
	}

	public override HashSet<string> RequiresStates() {
		return [_diveStateName];
	}

	public override void ReceiveTrigger(NakkiV2 nakki) {
		nakki.TrySwitchToState(_diveStateName);
	}

	public override void AiUpdate(NakkiV2 nakki) { }

	public override void EnterState(NakkiV2 nakki) { }

	public override void ExitState(NakkiV2 nakki) { }
}
