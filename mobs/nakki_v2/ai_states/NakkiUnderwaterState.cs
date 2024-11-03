using System.Collections.Generic;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiUnderwaterState : NakkiAiState {
	[Export] private Array<string> _pickOneOfTheseStatesWhenDoneDiving = [];

	[Export] private float _underwaterTime = 5.0f;
	[Export] private string _stateName = "underwater";
	private Timer? _diveTimer;
	private bool _isDoneDiving = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_diveTimer = GetNode<Timer>("Timer");
		_diveTimer.Timeout += () => {
			_isDoneDiving = true;
		};

		if (_pickOneOfTheseStatesWhenDoneDiving.Count == 0) {
			GD.PrintErr("Näkki's underwater state's pick-a-state-after-done-with-diving-list is empty!");
		}
	}

	public override string StateName() {
		return _stateName;
	}

	public override HashSet<string> RequiresStates() {
		return new(_pickOneOfTheseStatesWhenDoneDiving);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneDiving) {
			nakki.PlayEmergeFromWaterAnimation();
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		_rng.TryPickRandom(_pickOneOfTheseStatesWhenDoneDiving, out var name);
		if (name != null) {
			nakki.TrySwitchToState(name);
		} else {
			GD.PrintErr("Näkki's underwater state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneDiving = false;
		_diveTimer!.Stop();
		nakki.PlayDiveAnimation();
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki._detectionLevel = 0.0f;

		_diveTimer!.Stop();
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation == NakkiAnimation.Dive) {
			_diveTimer!.WaitTime = _underwaterTime;
			_diveTimer.Start();
			return;
		}

		if (animation == NakkiAnimation.EmergeFromWater) {
			SelectNewState(nakki);
			return;
		}
	}

	public override bool ShouldTickDetection() {
		return false;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
}
