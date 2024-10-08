using BogMonsterStuff;
using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public PackedScene? WaterSplash;

	private Player? player;

	public BogMonsterAIState ai = new MovementState(goingForward: true, 0.0f);

	public RandomNumberGenerator rng = new();

	private AnimationPlayer? animationPlayer;
	private Timer? underwaterCooldown;

	private RayCast2D? lineOfSight;

	public float detectionLevel;

	public NakkiAttack? attack;
	public AnimatedSprite2D? fakePlayer;
	public AnimatedSprite2D? hand;

	private bool playerWasKill = false;


	private BogMonsterStats? _stats;

	[Export]
	public BogMonsterStats stats {
		get => _stats ?? Util.TrustMeBro<BogMonsterStats>();
		set {
			_stats = value;
			UpdateConfigurationWarnings();
		}
	}

	[Export] public bool defaultStateIsWaitForTrigger;

	public override void _Ready() {
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		underwaterCooldown = GetNode<Timer>("UnderwaterCooldown");
		lineOfSight = GetNode<RayCast2D>("LineOfSight");
		attack = GetNode<NakkiAttack>("Attack");
		fakePlayer = GetNode<AnimatedSprite2D>("Attack/FakePlayer");
		hand = GetNode<AnimatedSprite2D>("Attack/Hand");

		var attackTimer = GetNode<Timer>("AttackTimer");
		attackTimer.WaitTime = stats.attackTime;

		ai = defaultState();

		this.Persistent().PlayerRespawned += () => {
			playerWasKill = false;
			animationPlayer?.Play("emerge_from_water", customSpeed: stats.emergeAnimationSpeed);
			// this fixes things
			ai = new MovementState(goingForward: true, stats.speed);
			fakePlayer.Visible = false;
		};
	}

	public BogMonsterAIState defaultState() {
		if (defaultStateIsWaitForTrigger) {
			return new WaitUntilTriggerIsTriggeredState();
		} else {
			return new MovementState(goingForward: true, stats.speed);
		}
	}

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;

		ai.doUpdate(this, delta);

		var shouldTickDetection = ai.shouldTickDetection();
		if (shouldTickDetection) {
			updateDetection(delta);
		}

		if (animationPlayer != null && animationPlayer.IsPlaying() && animationPlayer.CurrentAnimation == "attack") {
			SyncHandLocation(1.0f * delta);
		}
	}

	private void updateDetection(float delta) {
		var canSeePlayer = raycastToPlayer();

		if (canSeePlayer) {
			detectionLevel += stats.detectionGain * delta;
		} else {
			detectionLevel -= stats.detectionDecay * delta;
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

	public void goUnderwater(float timeMult = 1.0f) {
		var (min, max) = stats.underwaterTime;
		var underwaterTime = rng.RandfRange(min, max);
		ai = new UnderwaterState(underwaterTime * timeMult);
		animationPlayer?.Play("go_underwater");
		underwaterCooldown?.Start();
	}

	public void playGoUnderwaterAnimationThisIsVeryHackyThingDontUse() {
		animationPlayer?.Play("go_underwater");
	}

	public void playEmergeFromWaterAnimationThisIsVeryHackyThingDontUse() {
		animationPlayer?.Play("emerge_from_water");
	}

	public void goUnderwaterAnimationDone() {
		// HACK: if player was killed, just stay underwater
		if (playerWasKill) {
			return;
		}

		if (ai is UnderwaterState underwater) {
			underwater.animationDone = true;
		}
	}

	public void emergeFromWaterAtPosition(float progress) {
		ProgressRatio = progress;
		animationPlayer?.Play("emerge_from_water", customSpeed: stats.emergeAnimationSpeed);
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
		ai = new MovementState(goingForward, stats.speed);
	}

	public bool canGoUnderwater() {
		return underwaterCooldown?.IsStopped() ?? true;
	}

	public void playAttackAnimation() {
		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			if (hand is not null) {
				// HACK: scale instead of flipH to affect children, too
				hand.Scale = player.GlobalPosition.X < GlobalPosition.X
					? new(-1.0f, 1.0f)
					: new(1.0f, 1.0f);
			}

			if (WaterSplash is not null) {
				var splash = WaterSplash.Instantiate<Node2D>();
				GetParent().AddChild(splash);
				splash.GlobalPosition = player.GlobalPosition;
			}
		}

		animationPlayer?.Play("start_attack", customSpeed: stats.attackAnimationSpeed);
		ApplySlow();
		SyncHandLocation(1.0f);
	}

	private void finishAttack() {
		animationPlayer?.Play("finish_attack");
	}

	public void SyncHandLocation(float delta) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player || attack is null) {
			return;
		}

		var distance = attack.GlobalPosition.DistanceTo(player.GlobalPosition);
		attack.GlobalPosition = attack.GlobalPosition.MoveToward(player.GlobalPosition, distance * delta);
	}

	public void ApplySlow() {
		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			player.Slowed = true;
		}
	}

	public void ClearSlow() {
		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			player.Slowed = false;
		}
	}

	public void TryKillPlayer() {
		if (attack!.IsPlayerInDanger) {
			syncFakePlayerLocationAndHideAndKillPlayer();
		}
	}

	public void syncFakePlayerLocationAndHideAndKillPlayer() {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		player.setSpriteVisible(false);
		player.setMovementEnabled(false);
		SyncHandLocation(1.0f);
		fakePlayer!.Visible = true;

		player.die();
		playerWasKill = true;
	}

	public void attackAnimationDone() {
		ClearSlow();
		goUnderwater(0.25f);
	}
}
