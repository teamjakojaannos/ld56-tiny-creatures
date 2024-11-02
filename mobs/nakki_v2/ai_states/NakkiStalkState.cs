using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiStalkState : NakkiAiState {
	[Export] private float _stalkTime = 5.0f;
	private Timer? _timer;
	private bool _isDoneStalking;

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneStalking = true;
		};
	}

	public override string StateName() {
		return "stalk";
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

		var relative = GetPlayerXPositionRelative(nakki, player);
		nakki.SetProgressTarget(relative);
	}

	private static float GetPlayerXPositionRelative(NakkiV2 nakki, Player player) {
		var playerPosition = player.GlobalPosition;
		var myPosition = nakki.GlobalPosition;

		return playerPosition.X - myPosition.X;
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneStalking = false;

		_timer!.WaitTime = _stalkTime;
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
