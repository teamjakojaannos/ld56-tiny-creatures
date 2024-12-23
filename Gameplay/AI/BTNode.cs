using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

public abstract partial class BTNode : Node {
	public enum StatusCode {
		Success,
		Failure,
		Running,
	}

	public abstract StatusCode Tick();
}
