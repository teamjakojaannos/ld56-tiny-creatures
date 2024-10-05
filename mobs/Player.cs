using Godot;

public partial class Player : CharacterBody2D {
	[Export]
	public float speed = 300.0f;

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;

		var direction = Input.GetVector("left", "right", "up", "down");
		var velocity = direction * speed * delta;

		MoveAndCollide(velocity);
	}
}
