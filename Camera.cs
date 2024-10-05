using Godot;

public partial class Camera : Camera2D {

	[Export]
	public Node2D followTarget;

	public override void _Process(double delta) {
		if (followTarget != null) {
			Position = followTarget.Position;
		}
	}
}
