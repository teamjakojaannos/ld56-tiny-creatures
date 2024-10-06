using BogMonsterStuff;
using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public float speed = 45.0f;

	private Player? player;

	public BogMonsterAIState ai = new MovementState(goingForward: true, 0.0f);

	public RandomNumberGenerator rng = new();

	private AnimationPlayer? animationPlayer;
	private Timer? underwaterCooldown;

	private RayCast2D? lineOfSight;

	public float detectionLevel;
	[Export]
	public float detectionGain = 30.0f;
	[Export]
	public float detectionDecay = 10.0f;

	public Node2D? attack;
	public AnimatedSprite2D? fakePlayer;
	public AnimatedSprite2D? hand;

	public override void _Ready() {
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		underwaterCooldown = GetNode<Timer>("UnderwaterCooldown");
		lineOfSight = GetNode<RayCast2D>("LineOfSight");
		attack = GetNode<Node2D>("Attack");
		fakePlayer = GetNode<AnimatedSprite2D>("Attack/FakePlayer");
		hand = GetNode<AnimatedSprite2D>("Attack/Hand");

		ai = new MovementState(goingForward: true, speed);
	}

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;

		ai.doUpdate(this, delta);

		var shouldTickDetection = ai.shouldTickDetection();
		if (shouldTickDetection) {
			updateDetection(delta);
		}
	}

	private void updateDetection(float delta) {
		var canSeePlayer = raycastToPlayer();

		if (canSeePlayer) {
			detectionLevel += detectionGain * delta;
		} else {
			detectionLevel -= detectionDecay * delta;
		}

		detectionLevel = Mathf.Clamp(detectionLevel, 0.0f, 100.0f);

		ai.detectionLevelChanged(this);
	}

	private bool raycastToPlayer() {
		if (player == null || lineOfSight == null) {
			return false;
		}


		var playerPosition = player.GlobalPosition;
		// raycast wants target as relative to itself, not global
		var target = playerPosition - lineOfSight.GlobalPosition;
		lineOfSight.TargetPosition = target;

		lineOfSight.ForceRaycastUpdate();

		if (!lineOfSight.IsColliding()) {
			return false;
		}

		var collider = lineOfSight.GetCollider();
		return collider is Player;
	}


	public void sightConeEntered(Node2D node) {
		if (node is Player player) {
			this.player = player;
		}
	}

	public void sightConeExited(Node2D node) {
		if (node is Player) {
			player = null;
		}
	}

	public bool rollToGoUnderwater(float chance) {
		if (!canGoUnderwater()) {
			return false;
		}

		var goUnderwater = rng.Randf() < chance;
		if (goUnderwater) {
			this.goUnderwater();
			return true;
		}

		return false;
	}

	public void goUnderwater() {
		var (min, max) = BogMonsterStats.underwaterTime;
		var underwaterTime = rng.RandfRange(min, max);
		ai = new UnderwaterState(underwaterTime);
		animationPlayer?.Play("go_underwater");
		underwaterCooldown?.Start();
	}

	public void goUnderwaterAnimationDone() {
		if (ai is UnderwaterState underwater) {
			underwater.animationDone = true;
		}
	}

	public void emergeFromWaterAtPosition(float progress) {
		ProgressRatio = progress;
		animationPlayer?.Play("emerge_from_water");
	}

	public void emergeFromWaterNearPlayer() {
		float ratio = getPositionRatioThingyThatIsNearestToPlayer() ?? rng.Randf();
		emergeFromWaterAtPosition(ratio);
	}

	private float? getPositionRatioThingyThatIsNearestToPlayer() {
		if (GetParentOrNull<Path2D>() is not Path2D parent) {
			return null;
		}

		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return null;
		}

		var playerPosition = player.GlobalPosition;

		var points = parent.Curve.GetBakedPoints();
		var first = points[0] + parent.GlobalPosition;
		var last = points[points.Length - 1] + parent.GlobalPosition;

		// we only care about X coordinates as bog guy moves on X-axis
		// also I assume the curve goes left-to-right
		// so basically we're trying to find where player's position fits
		// between "first and last nodes in path"

		var smallest_x = Mathf.Min(first.X, last.X);
		var largest_x = Mathf.Max(first.X, last.X);
		var length = Mathf.Abs(largest_x - smallest_x);

		if (length == 0.0f) {
			return null;
		}

		var progress = (playerPosition.X - smallest_x) / length;
		return Mathf.Clamp(progress, 0.0f, 1.0f);
	}

	public void emergefromWaterAnimationDone() {
		var goingForward = Util.randomBool(rng);
		ai = new MovementState(goingForward, speed);
	}

	public bool canGoUnderwater() {
		return underwaterCooldown?.IsStopped() ?? true;
	}

	public void playAttackAnimation() {
		animationPlayer?.Play("attack");
	}

	public void syncFakePlayerLocationAndHideAndStopPlayer() {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		player.setSpriteVisible(false);
		player.setMovementEnabled(false);
		attack!.GlobalPosition = player.GlobalPosition;
		fakePlayer!.Visible = true;
	}

	public void killPlayer() {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is Player player) {
			player.die();
		}
	}

	public void attackAnimationDone() {
	}
}
