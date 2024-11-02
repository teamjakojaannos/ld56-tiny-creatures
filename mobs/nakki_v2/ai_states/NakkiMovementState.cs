using System.Collections.Generic;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiMovementState : NakkiAiState {
	[Export] private Array<string> _pickOneOfTheseStatesWhenDoneMoving = [];

	[Export] private float _moveTime = 5.0f;

	private Timer? _timer;
	private bool _isDoneMoving = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneMoving = true;
		};

		if (_pickOneOfTheseStatesWhenDoneMoving.Count == 0) {
			GD.PrintErr("Näkki's movement state's pick-a-state-after-done-with-movement-list is empty!");
		}
	}

	public override string StateName() {
		return "movement";
	}

	public override HashSet<string> RequiresStates() {
		return new HashSet<string>(_pickOneOfTheseStatesWhenDoneMoving);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneMoving || nakki.HasReachedTarget()) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		_rng.TryPickRandom(_pickOneOfTheseStatesWhenDoneMoving, out var name);
		if (name != null) {
			nakki.TrySwitchToState(name);
		} else {
			GD.PrintErr("Näkki's movement state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var newPosition = _rng.Randf();
		nakki.SetProgressRatioTarget(newPosition);

		_isDoneMoving = false;
		_timer!.WaitTime = _moveTime;
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
		_timer!.Stop();
	}
}
