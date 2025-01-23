using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiEmergeOnTriggerState : NakkiAiState {
	[Export] public float EmergeAnimationSpeed { get; set; } = 3.0f;
	[Export] public float EmergeDelay { get; set; } = 3.0f;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiAiState EnterStateAfterEmerge {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_enterStateAfterEmerge);
		set => this.SetExportProperty(ref _enterStateAfterEmerge, value);
	}
	private NakkiAiState? _enterStateAfterEmerge;

	private bool _emergeTimerDone = false;
	private bool _initialDiveAnimationDone = false;
	private bool _isEmerging = false;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void ReceiveTrigger(NakkiV2 nakki) {
		if (_isEmerging) {
			return;
		}

		GetTree().CreateTimer(EmergeDelay).Timeout += () => {
			_emergeTimerDone = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		var timersDone = _emergeTimerDone && _initialDiveAnimationDone;

		if (timersDone && !_isEmerging) {
			nakki.PlayEmergeFromWaterAnimation(EmergeAnimationSpeed);
			_isEmerging = true;
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_emergeTimerDone = false;
		_initialDiveAnimationDone = false;
		_isEmerging = false;

		nakki.PlayDiveAnimation(animationSpeed: 10.0f);
	}

	public override void ExitState(NakkiV2 nakki) { }

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
