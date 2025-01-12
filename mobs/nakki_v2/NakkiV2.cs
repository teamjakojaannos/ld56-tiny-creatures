using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiV2 : Path2D {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		if (Curve == null) {
			warnings = warnings.Append("Add a curve for näkki to move along (under 'Path2D' in inspector)").ToArray();
		} else if (Curve.PointCount == 0) {
			warnings = warnings.Append("Add points for näkki's movement path (Curve resource under 'Path2D' in inspector)").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiAiState DefaultState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_defaultState);
		set => this.SetExportProperty(ref _defaultState, value, notifyPropertyListChanged: true);
	}
	private NakkiAiState? _defaultState;

	[Export]
	[MustSetInEditor]
	public PathFollow2D NakkiEntity {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nakkiEntity);
		set => this.SetExportProperty(ref _nakkiEntity, value);
	}
	private PathFollow2D? _nakkiEntity;

	[Export]
	[MustSetInEditor]
	public Area2D Sightcone {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sightcone);
		set => this.SetExportProperty(ref _sightcone, value);
	}
	private Area2D? _sightcone;

	[Export]
	[MustSetInEditor]
	public RayCast2D LineOfSight {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_lineOfSight);
		set => this.SetExportProperty(ref _lineOfSight, value);
	}
	private RayCast2D? _lineOfSight;

	[Export]
	[MustSetInEditor]
	public AnimationPlayer AnimationPlayer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animationPlayer);
		set => this.SetExportProperty(ref _animationPlayer, value);
	}
	private AnimationPlayer? _animationPlayer;

	[Export]
	[MustSetInEditor]
	public Node2D Attack {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attack);
		set => this.SetExportProperty(ref _attack, value);
	}
	private Node2D? _attack;

	[Export]
	[MustSetInEditor]
	public Area2D DangerZone {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dangerZone);
		set => this.SetExportProperty(ref _dangerZone, value);
	}
	private Area2D? _dangerZone;

	[Export]
	[MustSetInEditor]
	public AnimatedSprite2D FakePlayer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_fakePlayer);
		set => this.SetExportProperty(ref _fakePlayer, value);
	}
	private AnimatedSprite2D? _fakePlayer;

	[Export]
	[MustSetInEditor]
	public AnimatedSprite2D Hand {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_hand);
		set => this.SetExportProperty(ref _hand, value);
	}
	private AnimatedSprite2D? _hand;


	public NakkiAiState CurrentState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_currentState);
		set {
			// IMPORTANT: the property will be null before _Ready is called.
			// Use backing field here to avoid an error on first write
			_currentState?.ExitState(this);
			_currentState = value;
			// it seems that Godot likes to reset node's fields when starting the game
			// use ? operator here to avoid errors on launch
			value?.EnterState(this);
		}
	}
	private NakkiAiState? _currentState;


	[ExportGroup("")]
	[Export] public float Speed { get; set; } = 50.0f;
	public float DetectionLevel { get; set; } = 0.0f;
	[Export] public float DetectionGain { get; set; } = 100.0f;
	[Export] public float DetectionDecay { get; set; } = 60.0f;


	private Timer? _attackTimer;
	private float? _targetProgress;
	private Player? _player;
	private bool _isPlayerInDanger = false;
	private bool _playerIsDead = false;


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		Sightcone.BodyEntered += SightConeEntered;
		Sightcone.BodyExited += SightConeExited;

		DangerZone.BodyEntered += DangerZoneEntered;
		DangerZone.BodyExited += DangerZoneExited;

		// TODO: add timer with code instead of in editor
		_attackTimer = GetNode<Timer>("AttackTimer");
		_attackTimer.Timeout += FinishAttack;


		ResetStateToDefault();

		this.Persistent().PlayerRespawned += () => {
			ResetStateToDefault();
		};
	}

	public void ResetStateToDefault() {
		_attackTimer.Stop();
		AnimationPlayer.Stop();
		AnimationPlayer.Play("RESET");

		_isPlayerInDanger = false;
		_playerIsDead = false;
		FakePlayer.Visible = false;
		Hand.Visible = false;
		_targetProgress = null;

		CurrentState = DefaultState;
	}

	public override void _PhysicsProcess(double ddelta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)ddelta;

		CurrentState.AiUpdate(this);

		var shouldTickDetection = CurrentState.ShouldTickDetection();
		if (shouldTickDetection) {
			UpdateDetection(delta);
		}

		MoveTowardsTarget(delta);
	}

	private void MoveTowardsTarget(float delta) {
		if (_targetProgress is not float target || HasReachedTarget()) {
			return;
		}

		var myProgress = NakkiEntity.Progress;
		var distanceToTarget = Mathf.Abs(target - myProgress);
		var maxMovement = Mathf.Abs(Speed * delta);

		var direction = myProgress < target ? 1.0f : -1.0f;
		var movement = direction * Mathf.Min(maxMovement, distanceToTarget);

		NakkiEntity.Progress += movement;
	}

	private void UpdateDetection(float delta) {
		var canSeePlayer = RaycastHitsPlayer();

		if (canSeePlayer) {
			DetectionLevel += DetectionGain * delta;
		} else {
			DetectionLevel -= DetectionDecay * delta;
		}

		DetectionLevel = Mathf.Clamp(DetectionLevel, 0.0f, 100.0f);

		CurrentState.DetectionLevelChanged(this);
	}

	private bool RaycastHitsPlayer() {
		if (_player == null) {
			return false;
		}

		var playerPosition = _player.GlobalPosition;
		// raycast wants target as relative to itself, not global
		var target = playerPosition - LineOfSight.GlobalPosition;
		LineOfSight.TargetPosition = target;
		LineOfSight.ForceRaycastUpdate();
		if (!LineOfSight.IsColliding()) {
			return false;
		}

		var collider = LineOfSight.GetCollider();
		return collider is Player;
	}

	public bool HasReachedTarget() {
		if (_targetProgress is not float target) {
			return true;
		}

		var threshold = 10.0f;
		var myProgress = NakkiEntity.Progress;
		return Mathf.Abs(myProgress - target) <= threshold;
	}

	public void SetProgressRatioTarget(float progressRatio) {
		var totalLength = Curve.GetBakedLength();

		_targetProgress = progressRatio * totalLength;
	}

	public void SetProgressTarget(float progress) {
		_targetProgress = progress;
	}

	public void TeleportToProgress(float progress) {
		NakkiEntity.Progress = progress;
		_targetProgress = null;
	}

	public void ClearMovementTarget() {
		_targetProgress = null;
	}

	public float PathLength => Curve.GetBakedLength();

	private void SightConeEntered(Node2D node) {
		if (node is Player player) {
			_player = player;
		}
	}

	private void SightConeExited(Node2D node) {
		if (node is Player) {
			_player = null;
		}
	}

	private void DangerZoneEntered(Node2D node) {
		if (node is Player) {
			_isPlayerInDanger = true;
		}
	}

	private void DangerZoneExited(Node2D node) {
		if (node is Player) {
			_isPlayerInDanger = false;
		}
	}

	/// <summary>
	/// Plays attack animation, which will start a timer. After 'attackTime' seconds
	/// näkki will finish the attack and kill player is they are in the danger zone.
	/// </summary>
	public void PlayAttackAnimation(float attackTime, float animationSpeed = 1.0f) {
		_attackTimer!.WaitTime = attackTime;
		AnimationPlayer.Play("start_attack", customSpeed: animationSpeed);
	}

	private void FinishAttack() {
		AnimationPlayer.Play("finish_attack");
	}

	private void AttackAnimationDone() {
		CurrentState.NakkiAnimationFinished(this, NakkiAnimation.Attack);
	}

	private void TryKillPlayer() {
		if (!_isPlayerInDanger) {
			return;
		}

		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		player.SetSpriteVisible(false);
		player.SetMovementEnabled(false);
		FakePlayer.Visible = true;

		player.Die();
		_playerIsDead = true;
	}

	public void PlayDiveAnimation(float animationSpeed = 1.0f) {
		AnimationPlayer.Play("go_underwater", customSpeed: animationSpeed);
	}

	private void GoUnderwaterAnimationDone() {
		CurrentState.NakkiAnimationFinished(this, NakkiAnimation.Dive);
	}

	public void PlayEmergeFromWaterAnimation(float animationSpeed = 1.0f) {
		AnimationPlayer.Play("emerge_from_water", customSpeed: animationSpeed);
	}

	private void EmergeFromWaterAnimationDone() {
		CurrentState.NakkiAnimationFinished(this, NakkiAnimation.EmergeFromWater);
	}

	public float GetPlayerXPositionRelative(Player player) {
		var playerPosition = player.GlobalPosition;
		return playerPosition.X - GlobalPosition.X;
	}

	public void PlayerEnteredTrigger() {
		CurrentState.ReceiveTrigger(this);
	}
}
