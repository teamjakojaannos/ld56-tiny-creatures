using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Characters.Wisp;

[Tool]
[GlobalClass]
[System.Obsolete("rewritten in gdscript, around just for reference for rewrite")]
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

	public void GoToSync(Vector2 target) {
		_goToTarget = target;
	}

	public void TeleportTo(Vector2 target, bool lockOn = false) {
		GlobalPosition = target;

		if (lockOn) {
			_goToTarget = target;
		}
	}

	public void Release() {
		_goToTarget = null;
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

		//ApplyMovementForces();

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
}
