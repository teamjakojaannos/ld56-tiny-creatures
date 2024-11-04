using System.Collections.Generic;
using System.Linq;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiMovementState : NakkiAiState {
	[Export] private Array<string> _pickOneOfTheseStatesWhenDoneMoving = [];

	[Export] private float _moveTime = 3.0f;
	[Export] private float _moveTimeVariation = 0.3f;
	[Export] private float _moveToPlayerChance = 0.2f;
	[Export] private string _stateName = "movement";

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
		return _stateName;
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
		var possibleStates = nakki.CanGoUnderwater()
			? _pickOneOfTheseStatesWhenDoneMoving
			: _pickOneOfTheseStatesWhenDoneMoving.Where(s => s != "underwater");

		_rng.TryPickRandom(possibleStates, out var name);
		if (name != null) {
			nakki.TrySwitchToState(name);
		} else {
			GD.PrintErr("Näkki's movement state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		var moveToPlayer = _rng.DiceRoll(_moveToPlayerChance);

		if (moveToPlayer && playerRef is Player player) {
			var relative = nakki.GetPlayerXPositionRelative(player);
			nakki.SetProgressTarget(relative);
		} else {
			var newPosition = _rng.Randf();
			nakki.SetProgressRatioTarget(newPosition);
		}

		_isDoneMoving = false;
		_timer!.WaitTime = _rng.RandomWithVariation(_moveTime, _moveTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
		_timer!.Stop();
	}
}
