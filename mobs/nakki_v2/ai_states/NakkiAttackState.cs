using Godot;

using System.Collections.Generic;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiAttackState : NakkiAiState {
	[Export] private PackedScene? _waterSplash;
	[Export] private float _attackTime = 1.0f;
	[Export] private float _animationSpeed = 1.0f;
	[Export] private string _stateName = "attack";

	public override void _Ready() {
		if (_waterSplash == null) {
			GD.PrintErr("You forgot to set water splash scene!");
		}
	}

	public override string StateName() {
		return _stateName;
	}

	public override HashSet<string> RequiresStates() {
		return [];
	}

	public override void AiUpdate(NakkiV2 nakki) {
	}

	public override void EnterState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		if (_waterSplash is not null) {
			var splash = _waterSplash.Instantiate<Node2D>();
			// we want to set position before making it visible (aka adding as a child)
			// so it isn't out of position for 1 frame
			splash.GlobalPosition = player.GlobalPosition - nakki.Position;
			nakki.AddChild(splash);
		}

		// HACK: scale instead of flipH to affect children, too
		nakki._hand!.Scale = player.GlobalPosition.X < nakki._nakkiEntity!.GlobalPosition.X
			? new(-1.0f, 1.0f)
			: new(1.0f, 1.0f);

		player.Slowed = true;
		nakki._attack!.GlobalPosition = player.GlobalPosition;
		nakki.PlayAttackAnimation(_attackTime, _animationSpeed);
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
