using Godot;

public partial class Chaser : RigidBody2D {

	[Export]
	public float speed = 150.0f;

	[Export(PropertyHint.Range, "-3.14,3.14,")]
	public float coneAngleOffset = Mathf.Pi / 2.0f;

	[Export]
	public float turnSpeedDegPerSec = 45.0f;

	private Node2D? chaseTarget = null;

	private Area2D? sightCone;

	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public AnimatedSprite2D? Sprite;


	private ChaserStuff.ChaserAI aiState = new ChaserStuff.IdleState();
	private RandomNumberGenerator rng = new();

	public override void _Ready() {
		sightCone = GetNode<Area2D>("SightCone");
	}

	public override void _Process(double _delta) {
		var delta = (float)_delta;
		aiState.doUpdate(this, delta);
	}

	public void moveTowards(Vector2 target, float delta) {
		var velocity = (target - GlobalPosition).Normalized() * speed * delta;
		MoveAndCollide(velocity);

		if (velocity.LengthSquared() > 0.0001f) {
			AnimPlayer!.Play(velocity.Y >= 0.0f ? "WalkFront" : "WalkBack");

			Sprite.FlipH = velocity.X > 0.0f;
		} else {
			AnimPlayer!.Play("Idle");
		}
	}

	public void turnTowardsTarget(Vector2 target, float delta) {
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

		startChase(player);
	}

	public void sightConeExited(Node2D node) {
		if (node is not Player player) {
			return;
		}

		startSeeking(player.GlobalPosition);
	}

	private void startChase(Player player) {
		aiState = new ChaserStuff.ChaseState(player);
	}

	private void startSeeking(Vector2 lastPosition) {
		aiState = new ChaserStuff.SeekState(lastPosition);
	}

	public void startWandering() {
		var (min, max) = ChaserStuff.ChaserStats.howFarNewTargetShouldBe;
		var randomPoint = Util.randomVector(rng, min, max);
		aiState = new ChaserStuff.WanderState(GlobalPosition + randomPoint);
	}

	public void startIdling() {
		aiState = new ChaserStuff.IdleState();
	}
}
