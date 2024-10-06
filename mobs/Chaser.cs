using ChaserStuff;
using Godot;

public partial class Chaser : RigidBody2D {

	[Export]
	public float speed = 150.0f;

	[Export(PropertyHint.Range, "-3.14,3.14,")]
	public float coneAngleOffset = Mathf.Pi / 2.0f;

	[Export]
	public float turnSpeedDegPerSec = 45.0f;

	private Area2D? sightCone;

	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public AnimatedSprite2D? Sprite;


	private ChaserAI aiState = new IdleState();
	public RandomNumberGenerator rng = new();

	private float? whatDirectionToLook = null;
	private Vector2? movementTarget = null;


	public override void _Ready() {
		sightCone = GetNode<Area2D>("SightCone");
	}

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;
		aiState.doUpdate(this, delta);

		turnHead(delta);
		var velocity = moveTowardsCurrentTarget(delta);

		clearTargetsIfCompleted();

		updateSprite(velocity);
	}

	private Vector2? moveTowardsCurrentTarget(float delta) {
		if (movementTarget is not Vector2 target) {
			return null;
		}

		var velocity = GlobalPosition.DirectionTo(target) * speed * delta;
		MoveAndCollide(velocity);
		return velocity;
	}

	private void updateSprite(Vector2? vel) {
		if (vel is not Vector2 velocity || velocity.LengthSquared() < 0.0001f) {
			AnimPlayer!.Play("Idle");
		} else {
			AnimPlayer!.Play(velocity.Y >= 0.0f ? "WalkFront" : "WalkBack");

			Sprite!.FlipH = velocity.X > 0.0f;
		}
	}

	public bool hasReachedMovementTarget() {
		if (movementTarget is not Vector2 pos) {
			return true;
		}

		const float closeEnough = 5.0f;
		return GlobalPosition.DistanceSquaredTo(pos) <= closeEnough;
	}

	public void setMovementTarget(Vector2 targetGlobalPos) {
		movementTarget = targetGlobalPos;
	}

	public void clearMovementTarget() {
		movementTarget = null;
	}

	public void setLookDirection(Vector2 targetGlobalPos) {
		var myPos = GlobalPosition;
		var desiredAngle = myPos.AngleToPoint(targetGlobalPos) + coneAngleOffset;

		whatDirectionToLook = desiredAngle;
	}

	public void clearLookTarget() {
		whatDirectionToLook = null;
	}

	public bool isDoneTurning() {
		const float closeEnough = 0.01f;

		if (whatDirectionToLook is not float desiredAngle) {
			return true;
		}

		var currentAngle = sightCone!.Rotation;
		var diff = Mathf.AngleDifference(currentAngle, desiredAngle);
		return Mathf.Abs(diff) <= closeEnough;
	}

	private void turnHead(float delta) {
		if (whatDirectionToLook is not float desiredAngle) {
			return;
		}

		var turnSpeedRadPerSec = Mathf.DegToRad(turnSpeedDegPerSec);
		var maxTurn = turnSpeedRadPerSec * delta;

		var currentAngle = sightCone!.Rotation;

		var rotationAmount = Mathf.AngleDifference(currentAngle, desiredAngle);
		if (Mathf.Abs(rotationAmount) > maxTurn) {
			rotationAmount = maxTurn * Mathf.Sign(rotationAmount);
		}

		sightCone.Rotation += rotationAmount;
	}

	private void clearTargetsIfCompleted() {
		if (isDoneTurning()) {
			clearLookTarget();
		}

		if (hasReachedMovementTarget()) {
			clearMovementTarget();
		}
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
		clearLookTarget();
		clearMovementTarget();

		aiState = new ChaseState(player);
	}

	private void startSeeking(Vector2 lastPosition) {
		clearLookTarget();
		clearMovementTarget();

		aiState = new SeekState(lastPosition);
	}

	public void startWandering() {
		clearLookTarget();
		clearMovementTarget();

		var (min, max) = ChaserStats.howFarNewTargetShouldBe;
		var randomPoint = Util.randomVector(rng, min, max);
		aiState = new WanderState(GlobalPosition + randomPoint);
	}

	public void startIdling() {
		clearLookTarget();
		clearMovementTarget();

		aiState = new IdleState();
	}
}
