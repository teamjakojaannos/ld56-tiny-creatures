using Godot;

namespace Jakojaannos.WisperingWoods;
public partial class NakkiEmergeOnTriggerState : NakkiAiState {
	[Export] private NakkiAiState? _enterStateAfterEmerge;
	[Export] private float _emergeAnimationSpeed = 3.0f;
	[Export] private float _emergeDelay = 3.0f;

	private Timer? _emergeOnTimeout;

	private bool _emergeTimerDone = false;
	private bool _initialDiveAnimationDone = false;
	private bool _isEmerging = false;

	public override void _Ready() {
		_emergeOnTimeout = GetNode<Timer>("Timer");
		_emergeOnTimeout.Timeout += () => {
			_emergeTimerDone = true;
		};

		if (_enterStateAfterEmerge == null) {
			GD.PrintErr("State to enter is null");
		}
	}

	public override void ReceiveTrigger(NakkiV2 nakki) {
		if (_isEmerging) {
			return;
		}

		_emergeOnTimeout!.WaitTime = _emergeDelay;
		_emergeOnTimeout.Start();
	}

	public override void AiUpdate(NakkiV2 nakki) {
		var timersDone = _emergeTimerDone && _initialDiveAnimationDone;

		if (timersDone && !_isEmerging) {
			nakki.PlayEmergeFromWaterAnimation(_emergeAnimationSpeed);
			_isEmerging = true;
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_emergeOnTimeout!.Stop();
		_emergeTimerDone = false;
		_initialDiveAnimationDone = false;
		_isEmerging = false;

		nakki.PlayDiveAnimation(animationSpeed: 10.0f);
	}

	public override void ExitState(NakkiV2 nakki) {
		_emergeOnTimeout!.Stop();
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation == NakkiAnimation.EmergeFromWater) {
			nakki.SwitchToState(_enterStateAfterEmerge!);
			return;
		}

		if (animation == NakkiAnimation.Dive) {
			_initialDiveAnimationDone = true;
			return;
		}
	}

	public override bool ShouldTickDetection() {
		return false;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
}
