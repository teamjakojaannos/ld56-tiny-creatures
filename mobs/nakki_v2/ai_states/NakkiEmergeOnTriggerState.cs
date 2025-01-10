using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiEmergeOnTriggerState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export]
	[MustSetInEditor]
	public NakkiAiState EnterStateAfterEmerge {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_enterStateAfterEmerge);
		set => this.SetExportProperty(ref _enterStateAfterEmerge, value, notifyPropertyListChanged: true);
	}
	private NakkiAiState? _enterStateAfterEmerge;

	[Export] private float _emergeAnimationSpeed = 3.0f;
	[Export] private float _emergeDelay = 3.0f;

	private Timer? _emergeOnTimeout;

	private bool _emergeTimerDone = false;
	private bool _initialDiveAnimationDone = false;
	private bool _isEmerging = false;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_emergeOnTimeout = GetNode<Timer>("Timer");
		_emergeOnTimeout.Timeout += () => {
			_emergeTimerDone = true;
		};
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
			nakki.CurrentState = EnterStateAfterEmerge;
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
