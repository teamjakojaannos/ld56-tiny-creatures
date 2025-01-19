using System;
using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;

namespace Jakojaannos.WisperingWoods.Characters.Wisp;

[Tool]
public partial class Wisp : Node2D {
	private TaskCompletionSource _goToTask = new();
	private Vector2? _goToTarget;

	public async Task GoTo(Vector2 target) {
		_goToTarget = target;
		_goToTask = new();

		await _goToTask.Task;
	}

	public async Task Inspect(IWispPointOfInterest.IInspectable target) {
		throw new NotImplementedException();
	}

	public async Task Interact(IWispPointOfInterest.IInteractable target) {
		throw new NotImplementedException();
	}

	public async Task InspectNothing() {
		throw new NotImplementedException();
	}

	public bool IsWithinInteractRange(IWispPointOfInterest target) {
		return GlobalPosition.DistanceTo(target.WispGlobalPosition) < 5.0f;
	}

	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		if (_goToTarget is Vector2 target) {
			GlobalPosition = GlobalPosition.MoveToward(target, 10.0f * (float)delta);

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
