using Godot;

using System.Collections.Generic;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiAttackState : NakkiAiState {

	[Export] private float _attackTime = 1.0f;

	public override void _Ready() {
	}

	public override string StateName() {
		return "attack";
	}

	public override HashSet<string> RequiresStates() {
		return [];
	}

	public override void AiUpdate(NakkiV2 nakki) {
	}

	public override void EnterState(NakkiV2 nakki) {
		// TODO: spawn water splash
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		// HACK: scale instead of flipH to affect children, too
		nakki._hand!.Scale = player.GlobalPosition.X < nakki.GlobalPosition.X
			? new(-1.0f, 1.0f)
			: new(1.0f, 1.0f);

		player.Slowed = true;
		nakki._attack!.GlobalPosition = player.GlobalPosition;
		nakki.PlayAttackAnimation(_attackTime);
	}

	public override void ExitState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is Player player) {
			player.Slowed = false;
		}
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }

	public override bool ShouldTickDetection() {
		return false;
	}
}
