using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiV2 : Path2D {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? Array.Empty<string>();

		if (Curve == null) {
			warnings = warnings.Append("Add a curve for näkki to move along (under 'Path2D' in inspector)").ToArray();
		} else if (Curve.PointCount == 0) {
			warnings = warnings.Append("Add points for näkki's movement path (Curve resource under 'Path2D' in inspector)").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] private float _speed = 50.0f;

	[Export]
	[MustSetInEditor]
	public NakkiAiState DefaultState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_defaultState);
		set => this.SetExportProperty(ref _defaultState, value, notifyPropertyListChanged: true);
	}
	private NakkiAiState? _defaultState;

	public NakkiAiState? CurrentState {
		get => _currentState;
		set {
			CurrentState?.ExitState(this);
			_currentState = value;
			CurrentState?.EnterState(this);
		}
	}
	private NakkiAiState? _currentState;

	public PathFollow2D? _nakkiEntity;
	private float? _targetProgress;

	private Player? _player;
	private RayCast2D? _lineOfSight;
	public float DetectionLevel { get; set; } = 0.0f;
	[Export] public float DetectionGain { get; set; } = 100.0f;
	[Export] public float DetectionDecay { get; set; } = 60.0f;

	private AnimationPlayer? _animationPlayer;
	private bool _isPlayerInDanger = false;
	private bool _playerIsDead = false;
	public Node2D? _attack;
	private Timer? _attackTimer;
	private AnimatedSprite2D? _fakePlayer;
	public AnimatedSprite2D? _hand;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_nakkiEntity = GetNode<PathFollow2D>("NäkkiEntity");
		var sightcone = _nakkiEntity.GetNode<Area2D>("SightCone");
		sightcone.BodyEntered += SightConeEntered;
		sightcone.BodyExited += SightConeExited;

		_lineOfSight = _nakkiEntity.GetNode<RayCast2D>("LineOfSight");

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_attackTimer = GetNode<Timer>("AttackTimer");
		_attackTimer.Timeout += FinishAttack;

		_attack = GetNode<Node2D>("Attack");

		var dangerZone = GetNode<Area2D>("Attack/DangerZoneSprite/DangerZone");
		dangerZone.BodyEntered += DangerZoneEntered;
		dangerZone.BodyExited += DangerZoneExited;

		_fakePlayer = GetNode<AnimatedSprite2D>("Attack/FakePlayer");
		_hand = GetNode<AnimatedSprite2D>("Attack/Hand");

		ResetStateToDefault();

		this.Persistent().PlayerRespawned += () => {
			_attackTimer.Stop();
			_animationPlayer.Stop();
			_animationPlayer.Play("RESET");

			_isPlayerInDanger = false;
			_playerIsDead = false;
			_fakePlayer.Visible = false;
			_hand.Visible = false;
			_targetProgress = null;

			ResetStateToDefault();
		};
	}

	public void ResetStateToDefault() {
		CurrentState = DefaultState;
	}

	public override void _PhysicsProcess(double ddelta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)ddelta;

		CurrentState!.AiUpdate(this);

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

		var myProgress = _nakkiEntity!.Progress;
		var distanceToTarget = Mathf.Abs(target - myProgress);
		var maxMovement = Mathf.Abs(_speed * delta);

		var direction = myProgress < target ? 1.0f : -1.0f;
		var movement = direction * Mathf.Min(maxMovement, distanceToTarget);

		_nakkiEntity.Progress += movement;
	}

	private void UpdateDetection(float delta) {
		var canSeePlayer = RaycastHitsPlayer();

		if (canSeePlayer) {
			DetectionLevel += DetectionGain * delta;
		} else {
			DetectionLevel -= DetectionDecay * delta;
		}

		DetectionLevel = Mathf.Clamp(DetectionLevel, 0.0f, 100.0f);

		CurrentState!.DetectionLevelChanged(this);
	}

	private bool RaycastHitsPlayer() {
		if (_player == null || _lineOfSight == null) {
			return false;
		}

		var playerPosition = _player.GlobalPosition;
		// raycast wants target as relative to itself, not global
		var target = playerPosition - _lineOfSight!.GlobalPosition;
		_lineOfSight.TargetPosition = target;
		_lineOfSight.ForceRaycastUpdate();
		if (!_lineOfSight.IsColliding()) {
			return false;
		}

		var collider = _lineOfSight.GetCollider();
		return collider is Player;
	}

	public bool HasReachedTarget() {
		if (_targetProgress is not float target) {
			return true;
		}

		var threshold = 10.0f;
		var myProgress = _nakkiEntity!.Progress;
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
		_nakkiEntity!.Progress = progress;
		_targetProgress = null;
	}

	public void ClearMovementTarget() {
		_targetProgress = null;
	}

	public float PathLength() {
		return Curve.GetBakedLength();
	}

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
		_animationPlayer!.Play("start_attack", customSpeed: animationSpeed);
	}

	private void FinishAttack() {
		_animationPlayer!.Play("finish_attack");
	}

	private void AttackAnimationDone() {
		CurrentState!.NakkiAnimationFinished(this, NakkiAnimation.Attack);
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
		_fakePlayer!.Visible = true;

		player.Die();
		_playerIsDead = true;
	}

	public void PlayDiveAnimation(float animationSpeed = 1.0f) {
		_animationPlayer!.Play("go_underwater", customSpeed: animationSpeed);
	}

	private void GoUnderwaterAnimationDone() {
		CurrentState?.NakkiAnimationFinished(this, NakkiAnimation.Dive);
	}

	public void PlayEmergeFromWaterAnimation(float animationSpeed = 1.0f) {
		_animationPlayer!.Play("emerge_from_water", customSpeed: animationSpeed);
	}

	private void EmergeFromWaterAnimationDone() {
		CurrentState?.NakkiAnimationFinished(this, NakkiAnimation.EmergeFromWater);
	}

	public float GetPlayerXPositionRelative(Player player) {
		var playerPosition = player.GlobalPosition;
		return playerPosition.X - GlobalPosition.X;
	}

	public void PlayerEnteredTrigger() {
		CurrentState?.ReceiveTrigger(this);
	}
}
