using Godot;

namespace Jakojaannos.WisperingWoods.Util;

public static class TreeAsyncExtension {
	/// <summary>
	/// Convenience wrapper for creating async delay. Should be preferred over
	/// `Task.Delay` for better Godot engine interop (`Task.Delay` operates in
	/// real-time whereas this implementation is based on `SceneTreeTimer`,
	/// which properly handles time scale, pause, etc.)
	/// </summary>
	public static SignalAwaiter CreateDelay(this SceneTree tree, float timeSec, bool processAlways = true, bool processInPhysics = false, bool ignoreTimeScale = false) {
		return tree.ToSignal(tree.CreateTimer(timeSec, processAlways, processInPhysics, ignoreTimeScale), SceneTreeTimer.SignalName.Timeout);
	}
}