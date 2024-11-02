using System.Collections.Generic;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiIdleState : NakkiAiState {
	[Export] private Array<string> _pickOneOfTheseStatesWhenDoneIdling = [];

	[Export] private float _idleTime = 2.0f;

	private Timer? _timer;
	private bool _isDoneIdling = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneIdling = true;
		};

		if (_pickOneOfTheseStatesWhenDoneIdling.Count == 0) {
			GD.PrintErr("Näkki's idle state's pick-a-state-after-done-with-idling-list is empty!");
		}
	}

	public override string StateName() {
		return "idle";
	}

	public override HashSet<string> RequiresStates() {
		return new HashSet<string>(_pickOneOfTheseStatesWhenDoneIdling);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneIdling) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		_rng.TryPickRandom(_pickOneOfTheseStatesWhenDoneIdling, out var name);
		if (name != null) {
			nakki.TrySwitchToState(name);
		} else {
			GD.PrintErr("Näkki's idle state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneIdling = false;

		_timer!.WaitTime = _idleTime;
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}
}
