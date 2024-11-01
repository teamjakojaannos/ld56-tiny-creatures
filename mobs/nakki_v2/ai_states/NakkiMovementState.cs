using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiMovementState : NakkiAiState {
	[Export] private Array<NakkiAiState> _nextStates = [];

	[Export] private float _moveTime = 5.0f;

	private Timer? _timer;
	private bool _isDoneMoving = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneMoving = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneMoving || nakki.HasReachedTarget()) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		_rng.TryPickRandom(_nextStates, out var next);
		if (next != null) {
			nakki.SwitchToState(next);
		} else {
			nakki.SwitchToState(this);
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
