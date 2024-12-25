
using Godot;

using Jakojaannos.WisperingWoods.Gameplay.AI;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
[GlobalClass]
public partial class SetPlayerPositionAsMoveTarget : BTNode {
	public override StatusCode Tick(AIState state, float delta) {
		var target = this.Persistent().Player.GlobalPosition;
		state.SetState("target", target);
		state.SetState("lastKnownTarget", target);
		return StatusCode.Success;
	}
}
