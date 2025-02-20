using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods.Characters.Wisp;

[Tool]
public partial class Wisp : Node2D {
	[Export]
	public PlayerCharacter? Player { get; set; }

	private TaskCompletionSource _goToTask = new();
	private Vector2? _goToTarget;

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

	public Vector2 WispTargetGlobalPosition => _goToTarget ?? this.Persistent().Player.WispFollowNode.GlobalPosition;

	public override void _Process(double ddelta) {
		if (Engine.IsEditorHint()) {
			return;
		}
		var delta = (float)ddelta;

		var moveSpeed = _goToTarget is null ? 10.0f : 2.5f;

		var distance = GlobalPosition.DistanceTo(WispTargetGlobalPosition);
		GlobalPosition = GlobalPosition.MoveToward(WispTargetGlobalPosition, distance * moveSpeed * delta);

		if (_goToTarget is Vector2 target) {
			if (!_goToTask.Task.IsCompleted) {
				var player = this.Persistent().Player;
				var distanceToPlayer = player.GlobalPosition.DistanceTo(GlobalPosition);
				if (distanceToPlayer > 320.0f) {
					_goToTask.SetCanceled();
				} else if (GlobalPosition.DistanceTo(target) < 5.0f) {
					_goToTask.SetResult();
				}
			}
		}
	}
}
