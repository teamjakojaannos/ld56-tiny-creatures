using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Characters.Wisp;

[Tool]
public partial class WispCharacter : RigidBody2D {
	[Export]
	public float MaxVelocity { get; set; } = 500.0f;

	[Export]
	[MustSetInEditor]
	[ExportCategory("Prewire")]
	public PlayerCharacter Player {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_player);
		set => this.SetExportProperty(ref _player, value);
	}
	private PlayerCharacter? _player;

	[Export]
	[MustSetInEditor]
	public WispTargetPosition FollowPosition {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_followPosition);
		set => this.SetExportProperty(ref _followPosition, value);
	}
	private WispTargetPosition? _followPosition;

	public Vector2? InteractTargetPosition => _goToTarget;

	private TaskCompletionSource _goToTask = new();
	private Vector2? _goToTarget;

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public async Task GoTo(Vector2 target) {
		_goToTarget = target;
		_goToTask = new();

		await _goToTask.Task;
	}

	public async Task Inspect(IWispPointOfInterest.IInspectable target) {
		GD.Print($"Inspected {(target as Node)?.Name ?? "Unknown"}");

		await GetTree().CreateDelay(2.5f);
		_goToTarget = null;
	}

	public async Task Interact(IWispPointOfInterest.IInteractable target) {
		GD.Print($"Interacted with {(target as Node)?.Name ?? "Unknown"}");

		await GetTree().CreateDelay(2.5f);
		_goToTarget = null;
	}

	public async Task InspectNothing() {
		GD.Print("Nothing to inspect.");

		await GetTree().CreateDelay(0.5f);
		_goToTarget = null;
	}

	public bool IsWithinInteractRange(IWispPointOfInterest target) {
		return GlobalPosition.DistanceTo(target.WispGlobalPosition) < 5.0f;
	}

	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		ApplyMovementForces();

		if (_goToTarget is Vector2 target) {
			if (!_goToTask.Task.IsCompleted) {
				var distanceToPlayer = Player.GlobalPosition.DistanceTo(GlobalPosition);
				if (distanceToPlayer > 320.0f) {
					_goToTask.SetCanceled();
				} else if (GlobalPosition.DistanceTo(target) < 5.0f) {
					_goToTask.SetResult();
				}
			}
		}
	}

	public Vector2 WispTargetGlobalPosition => _goToTarget ?? FollowPosition.GlobalPosition;

	private void ApplyMovementForces() {
		var isPlayerMoving = !Player.InputDirection.IsZeroApprox();
		if (isPlayerMoving) {
			LinearDamp = 1.5f;
		} else {
			LinearDamp = 2.0f;
		}

		var targetPosition = WispTargetGlobalPosition;

		var distanceSq = GlobalPosition.DistanceSquaredTo(targetPosition);
		var distanceRatio = distanceSq / FollowPosition.FollowDistance;
		var force = 10.0f * distanceRatio;

		var direction = GlobalPosition.DirectionTo(targetPosition);

		ApplyCentralForce(direction * force);
		FollowPosition.DebugRadius = Mathf.Sqrt(force);
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		state.LinearVelocity = state.LinearVelocity.LimitLength(MaxVelocity);
	}
}
