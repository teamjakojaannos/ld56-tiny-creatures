using System;
using System.Linq;

using BogMonsterStuff;

using Godot;

using Jakojaannos.WisperingWoods;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

[Tool]
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

	[Export]
	[MustSetInEditor]
	public BogMonsterStats Stats {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_stats);
		set => this.SetExportProperty(ref _stats, value);
	}
	private BogMonsterStats? _stats;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] public bool defaultStateIsWaitForTrigger;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		underwaterCooldown = GetNode<Timer>("UnderwaterCooldown");
		lineOfSight = GetNode<RayCast2D>("LineOfSight");
		attack = GetNode<NakkiAttack>("Attack");
		fakePlayer = GetNode<AnimatedSprite2D>("Attack/FakePlayer");
		hand = GetNode<AnimatedSprite2D>("Attack/Hand");

		var attackTimer = GetNode<Timer>("AttackTimer");
		attackTimer.WaitTime = Stats.attackTime;

		ai = DefaultState();

		this.Persistent().PlayerRespawned += () => {
			playerWasKill = false;
			animationPlayer?.Play("emerge_from_water", customSpeed: Stats.emergeAnimationSpeed);
			// this fixes things
			ai = new MovementState(goingForward: true, Stats.speed);
			fakePlayer.Visible = false;
		};
	}

	public BogMonsterAIState DefaultState() {
		if (defaultStateIsWaitForTrigger) {
			return new WaitUntilTriggerIsTriggeredState();
		} else {
			return new MovementState(goingForward: true, Stats.speed);
		}
	}

	public override void _PhysicsProcess(double _delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)_delta;

		ai.DoUpdate(this, delta);

		var shouldTickDetection = ai.ShouldTickDetection();
		if (shouldTickDetection) {
			UpdateDetection(delta);
		}

		if (animationPlayer != null && animationPlayer.IsPlaying() && animationPlayer.CurrentAnimation == "attack") {
			SyncHandLocation(1.0f * delta);
		}
	}

	private void UpdateDetection(float delta) {
		var canSeePlayer = RaycastToPlayer();

		if (canSeePlayer) {
			detectionLevel += Stats.detectionGain * delta;
		} else {
			detectionLevel -= Stats.detectionDecay * delta;
		}

		detectionLevel = Mathf.Clamp(detectionLevel, 0.0f, 100.0f);

		ai.DetectionLevelChanged(this);
	}

	private bool RaycastToPlayer() {
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

	public void SightConeEntered(Node2D node) {
		if (node is Player player) {
			this.player = player;
		}
	}

	public void SightConeExited(Node2D node) {
		if (node is Player) {
			player = null;
		}
	}

	public bool RollToGoUnderwater(float chance) {
		if (!CanGoUnderwater()) {
			return false;
		}

		var goUnderwater = rng.Randf() < chance;
		if (goUnderwater) {
			this.GoUnderwater();
			return true;
		}

		return false;
	}

	public void GoUnderwater(float timeMult = 1.0f) {
		var (min, max) = Stats.UnderwaterTime;
		var underwaterTime = rng.RandfRange(min, max);
		ai = new UnderwaterState(underwaterTime * timeMult);
		animationPlayer?.Play("go_underwater");
		underwaterCooldown?.Start();
	}

	public void PlayGoUnderwaterAnimationThisIsVeryHackyThingDontUse() {
		animationPlayer?.Play("go_underwater");
	}

	public void PlayEmergeFromWaterAnimationThisIsVeryHackyThingDontUse() {
		animationPlayer?.Play("emerge_from_water");
	}

	public void GoUnderwaterAnimationDone() {
		// HACK: if player was killed, just stay underwater
		if (playerWasKill) {
			return;
		}

		if (ai is UnderwaterState underwater) {
			underwater.animationDone = true;
		}
	}

	public void EmergeFromWaterAtPosition(float progress) {
		ProgressRatio = progress;
		animationPlayer?.Play("emerge_from_water", customSpeed: Stats.emergeAnimationSpeed);
	}

	public void EmergeFromWaterNearPlayer() {
		float ratio = GetPositionRatioThingyThatIsNearestToPlayer() ?? rng.Randf();
		EmergeFromWaterAtPosition(ratio);
	}

	private float? GetPositionRatioThingyThatIsNearestToPlayer() {
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

	public void EmergefromWaterAnimationDone() {
		var goingForward = rng.RandomBool();
		ai = new MovementState(goingForward, Stats.speed);
	}

	public bool CanGoUnderwater() {
		return underwaterCooldown?.IsStopped() ?? true;
	}

	public void PlayAttackAnimation() {
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

		animationPlayer?.Play("start_attack", customSpeed: Stats.attackAnimationSpeed);
		ApplySlow();
		SyncHandLocation(1.0f);
	}

	private void FinishAttack() {
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
			SyncFakePlayerLocationAndHideAndKillPlayer();
		}
	}

	public void SyncFakePlayerLocationAndHideAndKillPlayer() {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		player.SetSpriteVisible(false);
		player.SetMovementEnabled(false);
		SyncHandLocation(1.0f);
		fakePlayer!.Visible = true;

		player.Die();
		playerWasKill = true;
	}

	public void AttackAnimationDone() {
		ClearSlow();
		GoUnderwater(0.25f);
	}
}
