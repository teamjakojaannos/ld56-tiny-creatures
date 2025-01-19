using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Characters.Wisp;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.PlayerInput;

[Tool]
[RequireParent(typeof(Player))]
public partial class InteractionController : Node {
	[Export]
	public Wisp? Wisp { get; set; }

	private TaskCompletionSource _mouseReleasedTask = new();

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public async Task TryInteractWith(IInteractable target) {
		if (/* Wisp is null */ Wisp is not Wisp wisp) {
			return;
		}

		var didReachTarget = await wisp.GoTo(target.WispPosition);
		if (!didReachTarget) {
			return;
		}

		await WaitUntilMouseReleased();
		if (!wisp.IsWithinInteractRange(target)) {
			return;
		}

		wisp.Inspect(target);
	}

	private async Task WaitUntilMouseReleased() {
		await _mouseReleasedTask.Task;
	}

	public override void _Process(double delta) {
		// Reset mouse awaiter task when mouse is pressed
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			if (_mouseReleasedTask.Task.IsCompleted) {
				_mouseReleasedTask = new();
			}
		}
		// Mark mouse awaiter task done when mouse is released
		else {
			if (!_mouseReleasedTask.Task.IsCompleted) {
				_mouseReleasedTask.SetResult();
			}
		}
	}
}
