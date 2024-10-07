using Godot;

public partial class DestroyKiviTrigger : Area2D {

	[Export]
	public Node2D? objectToDestroy;

	public void bodyEntered(Node2D node) {
		if (node is Player) {
			objectToDestroy?.QueueFree();
		}
	}
}
