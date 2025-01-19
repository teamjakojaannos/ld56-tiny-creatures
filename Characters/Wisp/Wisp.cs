using System;
using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;

namespace Jakojaannos.WisperingWoods.Characters.Wisp;

public partial class Wisp : Node2D {
	private TaskCompletionSource<bool>? _goToTask;
	private Vector2? _goToTarget;

	public async Task<bool> GoTo(Vector2 target) {
		_goToTarget = target;
		_goToTask = new();

		return await _goToTask.Task;
	}

	public void Inspect(IInteractable target) {
		throw new NotImplementedException();
	}

	public bool IsWithinInteractRange(IInteractable target) {
		return GlobalPosition.DistanceTo(target.WispPosition) < 5.0f;
	}

	public override void _Process(double delta) {
		if (_goToTarget is Vector2 target && _goToTask is not null) {
			GlobalPosition = GlobalPosition.MoveToward(target, 10.0f * (float)delta);

			var player = this.Persistent().Player;
			var distanceToPlayer = player.GlobalPosition.DistanceTo(GlobalPosition);
			if (distanceToPlayer > 100.0f) {
				_goToTask.SetResult(false);
			} else if (_goToTarget is Vector2 goToTarget && GlobalPosition.DistanceTo(goToTarget) < 5.0f) {
				_goToTask.SetResult(true);
			}
		}
	}
}
