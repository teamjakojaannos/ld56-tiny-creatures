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

	[Export] public float _stalkThreshold = 40.0f;
	[Export] public float _attackThreshold = 100.0f;

	private AnimationPlayer? _animationPlayer;
	private bool _isPlayerInDanger = false;
	private bool _playerIsDead = false;
	public Node2D? _attack;
	private Timer? _attackTimer;
	private AnimatedSprite2D? _fakePlayer;
	public AnimatedSprite2D? _hand;
	private Timer? _diveCooldown;

	public override void _Ready() {
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

		_diveCooldown = GetNode<Timer>("DiveCooldown");

		if (Curve == null) {
			GD.PrintErr("You forgot to set path for Näkki!");
		}

		LoadStates();
		LoadOverrideStates();
		CheckAllRequiredStatesArePresent();
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

			_statesByName[name] = state;
		}
	}

	/// <summary>
	/// Adding a state as näkki's child will replace the default state. Useful
	/// for example if we want to override "attack" state and don't want to
	/// enable "editable children"
	/// </summary>
	private void LoadOverrideStates() {
		var children = GetChildren();
		foreach (var node in children) {
			if (node is NakkiAiState state) {
				_statesByName[state.StateName()] = state;
			}
		}
	}

	private void CheckAllRequiredStatesArePresent() {
		foreach (var (name, state) in _statesByName) {
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

	public void EnterStalkState() {
		TrySwitchToState("stalk");
	}

	public void EnterAttackState() {
		TrySwitchToState("attack");
	}

	public void EnterDiveState(float timeMult = 1.0f) {
		TrySwitchToState("underwater");

		if (CurrentState is not NakkiUnderwaterState underwater) {
			var expected = nameof(NakkiUnderwaterState);
			var className = CurrentState?.GetClass() ?? "<null>";
			GD.Print($"Unexpected diving state, expected {expected}, found {className}");
			return;
		}

		underwater.SetDiveTimeMult(timeMult);
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

	public void SetProgressTarget(float progress) {
		_targetProgress = progress;
	}

	public void ClearMovementTarget() {
		_targetProgress = null;
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
	public void PlayAttackAnimation(float attackTime) {
		_attackTimer!.WaitTime = attackTime;
		_animationPlayer!.Play("start_attack");
	}

	private void FinishAttack() {
		_animationPlayer!.Play("finish_attack");
	}

	private void AttackAnimationDone() {
		EnterDiveState(timeMult: 0.25f);
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

	public void PlayDiveAnimation() {
		_animationPlayer!.Play("go_underwater");
	}

	private void GoUnderwaterAnimationDone() {
		CurrentState?.NakkiAnimationFinished(this, NakkiAnimation.Dive);
	}

	public void PlayEmergeFromWaterAnimation() {
		_animationPlayer!.Play("emerge_from_water");
	}

	private void EmergeFromWaterAnimationDone() {
		CurrentState?.NakkiAnimationFinished(this, NakkiAnimation.EmergeFromWater);
	}

	public void StartDiveCooldown() {
		// restart timer if it was already running
		_diveCooldown!.Stop();
		_diveCooldown.Start();
	}

	public bool CanGoUnderwater() {
		return _diveCooldown!.IsStopped();
	}
}
