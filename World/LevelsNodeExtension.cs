using Godot;

namespace Jakojaannos.WisperingWoods.World;

public static class LevelsNodeExtension {
	private static Levels? _instance;

	public static Levels Levels(this Node node) {
		return _instance ??= node.GetTree().Root.GetNode<Levels>("/root/Levels");
	}
}
