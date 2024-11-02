using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiV2 : Path2D {
	[Export] private float _speed = 50.0f;
	[Export] private string _defaultState = "idle";

	private readonly Dictionary<string, NakkiAiState> _statesByName = [];

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

		LoadStates();
		ResetStateToDefault();
	}

	private void LoadStates() {
		var nodes = GetNode("States").GetChildren();
		foreach (var node in nodes) {
			if (node is not NakkiAiState state) {
				var expected = nameof(NakkiAiState);
				var nodeName = node.Name;
				GD.PrintErr($"Näkki has non-{expected} state '{nodeName}'.");
				continue;
			}

			var name = state.StateName();
			if (_statesByName.ContainsKey(name)) {
				GD.Print($"Warning, näkki has multiple states named '{name}'.");
			}

			_statesByName.Add(name, state);
		}

		// make sure all required states are present
		foreach (var node in nodes) {
			if (node is not NakkiAiState state) {
				continue;
			}

			var reqs = state.RequiresStates();
			foreach (var req in reqs) {
				if (!_statesByName.ContainsKey(req)) {
					var stateName = state.StateName();
					GD.PrintErr($"Näkki's state '{req}' (required by {stateName}) is not found");
				}
			}
		}
	}

	public void ResetStateToDefault() {
		var state = _statesByName.GetValueOrDefault(_defaultState);
		if (state != null) {
			CurrentState = state;
		} else {
			GD.PrintErr($"Cant find näkki's default state {_defaultState}");
		}
	}

	public void TrySwitchToState(string stateName) {
		var state = _statesByName.GetValueOrDefault(stateName);
		if (state != null) {
			CurrentState = state;
		} else {
			GD.PrintErr($"Cant find näkki's state '{stateName}'");
		}
	}

	public void EnterAlertState() {
		TrySwitchToState("alert");
	}

	public void EnterAttackState() {
		TrySwitchToState("attack");
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
