using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

[Tool]
public abstract partial class BTNode : Node {
	public enum StatusCode {
		Success,
		Failure,
		Running,
	}

	public abstract StatusCode Tick(AIState state, float delta);
}
