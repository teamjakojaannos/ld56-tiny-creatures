using Godot;

namespace Jakojaannos.WisperingWoods;

public static class PersistentExt {
	private static Node? s_instance;

	public static Node Persistent(this Node node) {
		return s_instance ??= node.GetTree().Root.GetNode("/root/Persistent");
	}
}
