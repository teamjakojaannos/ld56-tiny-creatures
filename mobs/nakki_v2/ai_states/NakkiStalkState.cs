using System.Collections.Generic;

using Godot;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiStalkState : NakkiAiState {
	[Export] private float _stalkTime = 5.0f;
	[Export] private float _stalkTimeVariation = 0.5f;
	[Export] private string _stateName = "stalk";
	private Timer? _timer;
	private bool _isDoneStalking;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneStalking = true;
		};
	}

	public override string StateName() {
		return _stateName;
	}

	public override HashSet<string> RequiresStates() {
		return ["idle", "attack"];
	}

	public override void AiUpdate(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			nakki.TrySwitchToState("idle");
			return;
		}

		var relative = nakki.GetPlayerXPositionRelative(player);
		nakki.SetProgressTarget(relative);
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneStalking = false;

		_timer!.WaitTime = _rng.RandomWithVariation(_stalkTime, _stalkTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		if (nakki._detectionLevel <= 0.0f) {
			nakki.TrySwitchToState("idle");
			return;
		}

		if (nakki._detectionLevel >= nakki._attackThreshold) {
			nakki.TrySwitchToState("attack");
			return;
		}
	}
}
