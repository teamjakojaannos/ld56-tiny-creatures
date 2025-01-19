using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Wisp;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.PlayerInput;

public static class InteractionControllerExt {
	private static InteractionController? s_instance;

	public static InteractionController InteractionController(this Node node) {
		return s_instance ??= node.Persistent().GetNode<InteractionController>("InteractionController");
	}
}

[Tool]
public partial class InteractionController : Node {
	[Export]
	public float InteractHoldTimeSeconds { get; set; } = 2.0f;

	[Export]
	public Wisp? Wisp { get; set; }

	public IWispPointOfInterest? CurrentPointOfInterest { get; private set; }

	private TaskCompletionSource<float> _mouseReleasedTask = new();
	private ulong _mousePressTimestamp;

	private readonly List<IWispPointOfInterest> _pointsOfInterest = [];

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public async Task TryInteractWith(IWispPointOfInterest target) {
		if (/* Wisp is null */ Wisp is not Wisp wisp) {
			return;
		}

		if (target.IsInvisible || target.IsInactive) {
			return;
		}

		try {
			CurrentPointOfInterest = target;

			var mouseHeldTask = WaitUntilMouseReleased();
			GD.Print("TRACKING MOUSE RELEASE, STARTING GOTO");

			await wisp.GoTo(target.WispGlobalPosition);

			GD.Print("GOTO FINISHED");

			var mouseHeldTime = await mouseHeldTask;
			if (!wisp.IsWithinInteractRange(target)) {
				GD.Print("WISP ISN'T IN RANGE TO INTERACT");
				return;
			}

			if (target is IWispPointOfInterest.IInteractable interactable && mouseHeldTime >= InteractHoldTimeSeconds) {
				GD.Print("INTERACTING");
				await wisp.Interact(interactable);
			} else if (target is IWispPointOfInterest.IInspectable inspectable) {
				GD.Print("INSPECTING");
				await wisp.Inspect(inspectable);
			} else {
				GD.PrintErr($"Unexpected POI type: {target.GetType().FullName}");
			}
		} catch (Exception e) {
			GD.PrintErr($"Something unexpected: {e.Message}");
			throw e;
		} finally {
			CurrentPointOfInterest = null;
		}
	}

	private async Task<float> WaitUntilMouseReleased() {
		return await _mouseReleasedTask.Task;
	}

	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		// Reset mouse awaiter task when mouse is pressed
		if (Input.IsActionJustPressed("interact")) {
			if (_mouseReleasedTask.Task.IsCompleted) {
				_mouseReleasedTask = new();
			}
			_mousePressTimestamp = Time.GetTicksMsec();

			if (_pointsOfInterest.Count > 0) {
				// FIXME: most reliable would be to find the active Camera2D and use its coordinate space instead of Player
				var mousePosition = this.Persistent().Player.GetGlobalMousePosition();
				var target = _pointsOfInterest
					.OrderBy(poi => poi.DistanceTo(mousePosition))
					.First();
				TryInteractWith(target).FireAndForget();
			}
		} else if (Input.IsActionJustReleased("interact")) {
			if (!_mouseReleasedTask.Task.IsCompleted) {
				var elapsedMillis = Time.GetTicksMsec() - _mousePressTimestamp;
				var elapsedSeconds = elapsedMillis * 0.001f;
				_mouseReleasedTask.SetResult(elapsedSeconds);
			}
		}
	}

	internal void TrackPointOfInterest(IWispPointOfInterest poi) {
		_pointsOfInterest.Add(poi);
	}

	internal void UntrackPointOfInterest(IWispPointOfInterest poi) {
		_pointsOfInterest.Remove(poi);
	}
}
