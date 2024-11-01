using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiV2 : Path2D {
	[Export] private float _speed = 50.0f;
	[Export] private NakkiAiState? _defaultState;

	private NakkiAiState? _currentState;
	private NakkiAiState? CurrentState {
		get => _currentState;
		set {
			CurrentState?.ExitState(this);
			_currentState = value;
			CurrentState?.EnterState(this);
		}
	}

	private PathFollow2D? _nakkiEntity;
	private float? _targetProgress;

	private Player? _player;
	private RayCast2D? _lineOfSight;
	public float _detectionLevel = 0.0f;
	[Export] private float _detectionGain = 100.0f;
	[Export] private float _detectionDecay = 60.0f;

	[Export] public float _alertThreshold = 40.0f;
	[Export] public float _attackThreshold = 100.0f;

	public override void _Ready() {
		_nakkiEntity = GetNode<PathFollow2D>("NäkkiEntity");
		var sightcone = _nakkiEntity.GetNode<Area2D>("SightCone");
		sightcone.BodyEntered += SightConeEntered;
		sightcone.BodyExited += SightConeExited;

		_lineOfSight = _nakkiEntity.GetNode<RayCast2D>("LineOfSight");

		ResetStateToDefault();
	}

	public void ResetStateToDefault() {
		if (_defaultState != null) {
			CurrentState = _defaultState;
			return;
		}

		// no default state given -> load first node
		var states = GetNode("States").GetChildren();
		if (states.Count == 0) {
			GD.PrintErr("Näkki has no default state set, and no states as children!");
			return;
		}

		var first = states[0];
		if (first is NakkiAiState aiState) {
			CurrentState = aiState;
			return;
		} else {
			var className = first.GetClass();
			var expected = nameof(NakkiAiState);
			GD.PrintErr($"Error loading Näkki's default state: found node of type {className}, expected {expected}.");
		}
	}

	public void SwitchToState(NakkiAiState newState) {
		CurrentState = newState;
	}

	public void EnterAlertState() {
		GD.Print("Näkki enters alerted state");
	}

	public void EnterAttackState() {
		GD.Print("Näkki enters attack state");
	}

	public override void _PhysicsProcess(double ddelta) {
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
			_detectionLevel += _detectionGain * delta;
		} else {
			_detectionLevel -= _detectionDecay * delta;
		}

		_detectionLevel = Mathf.Clamp(_detectionLevel, 0.0f, 100.0f);

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

	public void ClearMovementTarget() {
		_targetProgress = null;
	}

	public void SightConeEntered(Node2D node) {
		if (node is Player player) {
			_player = player;
		}
	}

	public void SightConeExited(Node2D node) {
		if (node is Player) {
			_player = null;
		}
	}
}
