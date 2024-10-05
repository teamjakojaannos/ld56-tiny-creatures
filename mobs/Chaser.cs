using Godot;

public partial class Chaser : RigidBody2D {

	[Export]
	public float speed = 150.0f;

	[Export(PropertyHint.Range, "-3.14,3.14,")]
	public float coneAngleOffset = Mathf.Pi / 2.0f;

	private Node2D chaseTarget = null;

	private Area2D sightCone;


	public override void _Ready() {
		sightCone = GetNode<Area2D>("SightCone");
	}

	public override void _Process(double _delta) {
		var delta = (float)_delta;
		if (chaseTarget != null) {
			moveTowards(chaseTarget.Position, delta);
			lookTowardsTarget(chaseTarget.Position);
		}
	}

	private void moveTowards(Vector2 target, float delta) {
		var velocity = (target - Position).Normalized() * speed * delta;
		MoveAndCollide(velocity);
	}

	private void lookTowardsTarget(Vector2 target) {
		var angle = (target - Position).Angle() + coneAngleOffset;
		sightCone.Rotation = angle;
	}


	public void sightConeEntered(Node2D node) {
		if (node is not Player player) {
			return;
		}

		chaseTarget = player;
	}

	public void sightConeExited(Node2D node) {
		if (node is not Player) {
			return;
		}

		chaseTarget = null;
	}
}
