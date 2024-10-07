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

	private Vector2? whereToLookAt = null;

	private NavigationAgent2D? navigationAgent;
	private bool navigationSetupDone = false;

	public override void _Ready() {
		sightCone = GetNode<Area2D>("SightCone");
		navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");

		Callable.From(ActorSetup).CallDeferred();
	}

	private async void ActorSetup() {
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		navigationSetupDone = true;
	}

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;
		aiState.doUpdate(this, delta);

		turnHead(delta);
		var velocity = moveTowardsCurrentTarget(delta);

		if (hasReachedMovementTarget()) {
			clearMovementTarget();
		}

		updateSprite(velocity);
	}

	private Vector2? moveTowardsCurrentTarget(float delta) {
		if (navigationAgent!.IsNavigationFinished()) {
			return null;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = navigationAgent.GetNextPathPosition();
		var velocity = currentAgentPosition.DirectionTo(nextPathPosition) * speed * delta;
		MoveAndCollide(velocity);
		lookTowardsMovement(nextPathPosition);
		return velocity;
	}

	private void lookTowardsMovement(Vector2 target) {
		if (whereToLookAt is not null) {
			// specific target overrides "look towards movement"
			return;
		}

		clearLookTarget();
		var desiredAngle = GlobalPosition.AngleToPoint(target) + coneAngleOffset;
		sightCone!.Rotation = desiredAngle;
	}

	private void updateSprite(Vector2? vel) {
		if (vel is not Vector2 velocity || velocity.LengthSquared() < 0.0001f) {
			const float fullCircle = Mathf.Pi * 2.0f;
			const float halfCircle = fullCircle / 2.0f;
			const float quarterCircle = fullCircle / 4.0f;

			// this probably assumes that sight cone points upwards
			// what we're trying to do here: if the angle is between -45deg and +45 deg -> it looks up
			// 45-135 -> right etc
			var angle = sightCone!.Rotation;
			angle -= quarterCircle / 2.0f;
			angle = (angle + fullCircle) % fullCircle;
			var quarter = Mathf.FloorToInt(angle / quarterCircle);

			if (quarter == 0 || quarter == 2) {
				AnimPlayer!.Play("IdleFront");
			} else {
				AnimPlayer!.Play("IdleBack");
			}

			// here we're figuring if angle is between 0 and 180deg, or 180-360
			var angle2 = sightCone!.Rotation;
			angle2 = (angle2 + fullCircle) % fullCircle;

			var half = Mathf.Floor(angle2 / halfCircle);
			Sprite!.FlipH = half == 0;

		} else {
			AnimPlayer!.Play(velocity.Y >= 0.0f ? "WalkFront" : "WalkBack");
			Sprite!.FlipH = velocity.X > 0.0f;
		}
	}

	public bool hasReachedMovementTarget() {
		return navigationAgent!.IsNavigationFinished();
	}

	public void setMovementTarget(Vector2 targetGlobalPos) {
		if (!navigationSetupDone) {
			GD.Print("Trying to add target before navigation setup is done");
			return;
		}

		navigationAgent!.TargetPosition = targetGlobalPos;
	}

	public void clearMovementTarget() {
		// is this correct way to stop?
		navigationAgent!.TargetPosition = GlobalPosition;
	}

	public void setLookTarget(Vector2 targetGlobalPos, bool turnInstantly = false) {
		whereToLookAt = targetGlobalPos;

		if (turnInstantly) {
			var myPos = GlobalPosition;
			var desiredAngle = myPos.AngleToPoint(targetGlobalPos) + coneAngleOffset;
			sightCone!.Rotation = desiredAngle;
		}
	}

	public void clearLookTarget() {
		whereToLookAt = null;
	}

	public bool isDoneTurning() {
		const float closeEnough = 0.01f;

		if (whereToLookAt is not Vector2 point) {
			return true;
		}

		var myPos = GlobalPosition;
		var desiredAngle = myPos.AngleToPoint(point) + coneAngleOffset;
		var currentAngle = sightCone!.Rotation;
		var diff = Mathf.AngleDifference(currentAngle, desiredAngle);
		return Mathf.Abs(diff) <= closeEnough;
	}

	private void turnHead(float delta) {
		if (whereToLookAt is not Vector2 point) {
			return;
		}

		var myPos = GlobalPosition;
		var desiredAngle = myPos.AngleToPoint(point) + coneAngleOffset;

		var turnSpeedRadPerSec = Mathf.DegToRad(turnSpeedDegPerSec);
		var maxTurn = turnSpeedRadPerSec * delta;

		var currentAngle = sightCone!.Rotation;

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
