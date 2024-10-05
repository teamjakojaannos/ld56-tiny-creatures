using Godot;

public partial class Chaser : RigidBody2D {

	[Export]
	public float speed = 150.0f;

	[Export(PropertyHint.Range, "-3.14,3.14,")]
	public float coneAngleOffset = Mathf.Pi / 2.0f;

	[Export]
	public float turnSpeedDegPerSec = 45.0f;

	private Node2D chaseTarget = null;

	private Area2D sightCone;


	public override void _Ready() {
		sightCone = GetNode<Area2D>("SightCone");
	}

	public override void _Process(double _delta) {
		var delta = (float)_delta;
		if (chaseTarget != null) {
			moveTowards(chaseTarget.GlobalPosition, delta);
			turnTowardsTarget(chaseTarget.GlobalPosition, delta);
		}
	}

	private void moveTowards(Vector2 target, float delta) {
		var velocity = (target - GlobalPosition).Normalized() * speed * delta;
		MoveAndCollide(velocity);
	}

	private void turnTowardsTarget(Vector2 target, float delta) {


		var turnSpeedRadPerSec = Mathf.DegToRad(turnSpeedDegPerSec);
		var maxTurn = turnSpeedRadPerSec * delta;

		var myPos = GlobalPosition;

		var desiredAngle = myPos.AngleToPoint(target) + coneAngleOffset;
		var currentAngle = sightCone.Rotation;
		var rotationAmount = Mathf.AngleDifference(currentAngle, desiredAngle);
		if (Mathf.Abs(rotationAmount) > maxTurn) {
			rotationAmount = maxTurn * Mathf.Sign(rotationAmount);
		}

		sightCone.Rotation += rotationAmount;
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
