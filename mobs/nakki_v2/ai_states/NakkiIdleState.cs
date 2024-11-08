using System.Linq;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiIdleState : NakkiAiState {
	[Export] private Array<NakkiAiState> _pickOneOfTheseStatesWhenDoneIdling = [];

	[Export] private float _idleTime = 2.0f;
	[Export] private float _idleTimeVariation = 0.5f;
	[Export] private NakkiStalkState? _stalkState;
	[Export] private NakkiAttackState? _attackState;

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

		if (_stalkState == null) {
			GD.PrintErr("Stalk state is null");
		}

		if (_attackState == null) {
			GD.PrintErr("Attack state is null");
		}
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneIdling) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		var possibleStates = _pickOneOfTheseStatesWhenDoneIdling.Where(s => s.IsStateReady(nakki));

		_rng.TryPickRandom(possibleStates, out var state);
		if (state != null) {
			nakki.SwitchToState(state);
		} else {
			GD.PrintErr("Näkki's idle state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneIdling = false;

		_timer!.WaitTime = _rng.RandomWithVariation(_idleTime, _idleTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		StalkOrAttack(nakki, _attackState!, _stalkState!);
	}
}
