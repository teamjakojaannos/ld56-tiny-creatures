using System.Linq;

using ChaserStuff;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods;
using Jakojaannos.WisperingWoods.Audio;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

[Tool]
public partial class Chaser : RigidBody2D {
	public float speed;

	[Export]
	public float acceleration = 15.0f;
	[Export]
	public float baseSpeed = 60.0f;
	[Export]
	public float maxSpeed = 200.0f;

	[Export(PropertyHint.Range, "-3.14,3.14,")]
	public float coneAngleOffset = Mathf.Pi / 2.0f;

	[Export]
	public float turnSpeedDegPerSec = 45.0f;

	private Node2D? sightConeRoot;
	private Area2D? sightConeSmall;
	private Area2D? sightConeMedium;
	private Area2D? sightConeLarge;

	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public AnimatedSprite2D? Sprite;

	private ChaserAI aiState = new IdleState();
	internal RandomNumberGenerator rng = new();

	private Vector2? whereToLookAt = null;

	private NavigationAgent2D? navigationAgent;
	private bool navigationSetupDone = false;

	private RayCast2D? lineOfSight;
	private Node2D? _chaseTarget;
	private bool isAttacking;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public RandomAudioStreamPlayer2D Footsteps {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footsteps);
		set => this.SetExportProperty(ref _footsteps, value);
	}
	private RandomAudioStreamPlayer2D? _footsteps;

	[Export]
	[MustSetInEditor]
	public Timer FootstepsTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footstepsTimer);
		set => this.SetExportProperty(ref _footstepsTimer, value);
	}
	private Timer? _footstepsTimer;


	[Export]
	[MustSetInEditor]
	public RandomAudioStreamPlayer AttackSounds {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackSounds);
		set => this.SetExportProperty(ref _attackSounds, value);
	}

	private RandomAudioStreamPlayer? _attackSounds;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		sightConeRoot = GetNode<Node2D>("SightCone");
		navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");
		lineOfSight = GetNode<RayCast2D>("LineOfSight");

		sightConeSmall = sightConeRoot.GetNode<Area2D>("SightConeSmall");
		sightConeMedium = sightConeRoot.GetNode<Area2D>("SightConeMedium");
		sightConeLarge = sightConeRoot.GetNode<Area2D>("SightConeLarge");

		speed = baseSpeed;

		if (Engine.IsEditorHint()) {
			return;
		}

		Callable.From(ActorSetup).CallDeferred();

		var player = this.Persistent().Player;
		player.LightLevelChanged += PlayerLightLevelChanged;
		ActivateSightCone(player.lightLevel);

		this.Persistent().PlayerRespawned += () => {
			isAttacking = false;
			aiState = new IdleState();
		};

		if (FootstepsTimer is not null) {
			FootstepsTimer.Timeout += () => {
				Footsteps?.Play();
			};
		}
	}

	private async void ActorSetup() {
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		navigationSetupDone = true;
	}

	public override void _PhysicsProcess(double _delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)_delta;

		if (isAttacking) {
			return;
		}

		var seesPlayer = RaycastToChaseTarget();
		if (seesPlayer) {
			if (aiState is not ChaseState) {
				StartChase(_chaseTarget!);
			}
		} else {
			if (aiState is ChaseState chase) {
				StartSeeking(chase.LastKnownTargetPosition);
			}
		}

		aiState.DoUpdate(this, delta);
		UpdateSpeed(delta);

		TurnHead(delta);
		var velocity = MoveTowardsCurrentTarget(delta);

		if (velocity is not null && (velocity?.LengthSquared()) < 0.01f) {
			FootstepsTimer?.Stop();
		} else if (FootstepsTimer!.IsStopped()) {
			FootstepsTimer.Start();
		}

		if (HasReachedMovementTarget()) {
			ClearMovementTarget();
		}

		UpdateSprite(velocity);
	}

	private void UpdateSpeed(float delta) {
		var isChasing = aiState is ChaseState;
		var sign = isChasing ? +1.0f : -1.0f;
		speed += acceleration * delta * sign;
		speed = Mathf.Clamp(speed, baseSpeed, maxSpeed);
	}

	private Vector2? MoveTowardsCurrentTarget(float delta) {
		if (navigationAgent!.IsNavigationFinished()) {
			return null;
		}

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = navigationAgent.GetNextPathPosition();
		var velocity = currentAgentPosition.DirectionTo(nextPathPosition) * speed * delta;
		MoveAndCollide(velocity);
		LookTowardsMovement(nextPathPosition);
		return velocity;
	}

	private void LookTowardsMovement(Vector2 target) {
		if (whereToLookAt is not null) {
			// specific target overrides "look towards movement"
			return;
		}

		ClearLookTarget();
		var desiredAngle = GlobalPosition.AngleToPoint(target) + coneAngleOffset;
		sightConeRoot!.Rotation = desiredAngle;
	}

	private void UpdateSprite(Vector2? vel) {
		if (vel is not Vector2 velocity || velocity.LengthSquared() < 0.0001f) {
			const float fullCircle = Mathf.Pi * 2.0f;
			const float halfCircle = fullCircle / 2.0f;
			const float quarterCircle = fullCircle / 4.0f;

			// this probably assumes that sight cone points upwards
			// check if angle is between [-90, 90]
			var angle = sightConeRoot!.Rotation;
			angle -= quarterCircle;
			angle = (angle + fullCircle) % fullCircle;
			var topOrBack = Mathf.FloorToInt(angle / halfCircle);

			if (topOrBack == 0) {
				AnimPlayer!.Play("IdleFront");
			} else {
				AnimPlayer!.Play("IdleBack");
			}

			// here we're figuring if angle is between 0 and 180deg, or 180-360
			var angle2 = sightConeRoot!.Rotation;
			angle2 = (angle2 + fullCircle) % fullCircle;

			var half = Mathf.Floor(angle2 / halfCircle);
			Sprite!.FlipH = half == 0;

		} else {
			AnimPlayer!.Play(velocity.Y >= 0.0f ? "WalkFront" : "WalkBack");
			Sprite!.FlipH = velocity.X > 0.0f;
		}
	}

	public bool HasReachedMovementTarget() {
		return navigationAgent!.IsNavigationFinished();
	}

	public void SetMovementTarget(Vector2 targetGlobalPos) {
		if (!navigationSetupDone) {
			GD.Print("Trying to add target before navigation setup is done");
			return;
		}

		navigationAgent!.TargetPosition = targetGlobalPos;
	}

	public void ClearMovementTarget() {
		// is this correct way to stop?
		navigationAgent!.TargetPosition = GlobalPosition;
	}

	public void SetLookTarget(Vector2 targetGlobalPos, bool turnInstantly = false) {
		whereToLookAt = targetGlobalPos;

		if (turnInstantly) {
			var myPos = GlobalPosition;
			var desiredAngle = myPos.AngleToPoint(targetGlobalPos) + coneAngleOffset;
			sightConeRoot!.Rotation = desiredAngle;
		}
	}

	public void ClearLookTarget() {
		whereToLookAt = null;
	}

	public bool IsDoneTurning() {
		const float closeEnough = 0.01f;

		if (whereToLookAt is not Vector2 point) {
			return true;
		}

		var myPos = GlobalPosition;
		var desiredAngle = myPos.AngleToPoint(point) + coneAngleOffset;
		var currentAngle = sightConeRoot!.Rotation;
		var diff = Mathf.AngleDifference(currentAngle, desiredAngle);
		return Mathf.Abs(diff) <= closeEnough;
	}

	private void TurnHead(float delta) {
		if (whereToLookAt is not Vector2 point) {
			return;
		}

		var myPos = GlobalPosition;
		var desiredAngle = myPos.AngleToPoint(point) + coneAngleOffset;

		var turnSpeedRadPerSec = Mathf.DegToRad(turnSpeedDegPerSec);
		var maxTurn = turnSpeedRadPerSec * delta;

		var currentAngle = sightConeRoot!.Rotation;

		var rotationAmount = Mathf.AngleDifference(currentAngle, desiredAngle);
		if (Mathf.Abs(rotationAmount) > maxTurn) {
			rotationAmount = maxTurn * Mathf.Sign(rotationAmount);
		}

		sightConeRoot.Rotation += rotationAmount;
	}

	private bool RaycastToChaseTarget() {
		if (_chaseTarget == null) {
			return false;
		}

		var targetPosition = _chaseTarget.GlobalPosition;
		// raycast wants target as relative to itself, not global
		var target = targetPosition - lineOfSight!.GlobalPosition;
		lineOfSight.TargetPosition = target;

		lineOfSight.ForceRaycastUpdate();

		if (!lineOfSight.IsColliding()) {
			return false;
		}

		var collider = lineOfSight.GetCollider();
		return collider is PlayerCharacter;
	}

	enum SightConeSize {
		Small, Medium, Large
	}

	public void EnterSightConeSmall(Node2D node) { SightConeEntered(node, SightConeSize.Small); }
	public void EnterSightConeMedium(Node2D node) { SightConeEntered(node, SightConeSize.Medium); }
	public void EnterSightConeLarge(Node2D node) { SightConeEntered(node, SightConeSize.Large); }
	public void ExitSightConeSmall(Node2D node) { SightConeExited(node, SightConeSize.Small); }
	public void ExitSightConeMedium(Node2D node) { SightConeExited(node, SightConeSize.Medium); }
	public void ExitSightConeLarge(Node2D node) { SightConeExited(node, SightConeSize.Large); }

	private void SightConeEntered(Node2D node, SightConeSize _) {
		if (node is not PlayerCharacter player) {
			return;
		}

		this._chaseTarget = player;
	}

	private void SightConeExited(Node2D node, SightConeSize _) {
		if (node is not PlayerCharacter) {
			return;
		}

		_chaseTarget = null;
	}

	private void StartChase(Node2D target) {
		ClearLookTarget();
		ClearMovementTarget();

		AttackSounds?.Play();
		this.Jukebox().StartChase();
		aiState = new ChaseState(target);
	}

	private void StartSeeking(Vector2 lastPosition) {
		ClearLookTarget();
		ClearMovementTarget();

		this.Jukebox().StopChase();
		aiState = new SeekState(lastPosition);
	}

	public void StartWandering() {
		ClearLookTarget();
		ClearMovementTarget();

		var randomPoint = GetRandomNavigationPoint();
		aiState = new WanderState(randomPoint);
	}

	private Vector2 GetRandomNavigationPoint() {
		var roots = GetTree().GetNodesInGroup("MarkoMarkersRoot");
		var allPositions = new Array<Vector2>();

		foreach (var root in roots) {
			var children = root.GetChildren();
			foreach (var child in children) {
				if (child is Marker2D marker) {
					allPositions.Add(marker.GlobalPosition);
				}
			}
		}

		if (allPositions.Count == 0) {
			GD.Print("Can't find any navigation nodes for Marko, picking random point!");
			var randomPoint = rng.RandomVector(ChaserStats.wanderToNewTargetInRange);
			return randomPoint + GlobalPosition;
		} else {
			return allPositions.PickRandom();
		}
	}

	public void StartIdling() {
		ClearLookTarget();
		ClearMovementTarget();

		aiState = new IdleState();
	}

	public void EnteredKillZone(Node2D node) {
		if (node is PlayerCharacter player) {
			AttackPlayer(player);
		}
	}

	private void AttackPlayer(PlayerCharacter player) {
		AttackSounds?.Play();
		isAttacking = true;
		player.SetSpriteVisible(false);
		player.SetMovementEnabled(false);
		AnimPlayer!.Play("attack");
	}

	private void KillPlayer() {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not PlayerCharacter player) {
			return;
		}

		player.Die();
	}

	private void PlayerLightLevelChanged(int newLightLevel) {
		ActivateSightCone(newLightLevel);
	}

	private void ActivateSightCone(int lightLevel) {
		var enableSmall = lightLevel <= 1;
		var enableMedium = lightLevel == 2;
		var enableLarge = lightLevel >= 3;

		sightConeSmall!.Visible = enableSmall;
		sightConeSmall!.Monitoring = enableSmall;

		sightConeMedium!.Visible = enableMedium;
		sightConeMedium!.Monitoring = enableMedium;

		sightConeLarge!.Visible = enableLarge;
		sightConeLarge!.Monitoring = enableLarge;
	}
}
