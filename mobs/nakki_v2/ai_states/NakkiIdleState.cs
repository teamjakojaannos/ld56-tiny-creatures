using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiIdleState : NakkiAiState {
	[Export] private Array<NakkiAiState> _nextStates = [];

	[Export] private float _idleTime = 2.0f;

	private Timer? _timer;
	private bool _isDoneIdling = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneIdling = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneIdling) {
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
		_isDoneIdling = false;

		_timer!.WaitTime = _idleTime;
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}
}
