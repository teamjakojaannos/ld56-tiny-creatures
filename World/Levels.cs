using Godot;

namespace Jakojaannos.WisperingWoods.World;

public static class LevelsExt {
	private static Levels? _instance;

	public static Levels Levels(this Node node) {
		return _instance ??= node.GetTree().Root.GetNode<Levels>("/root/Levels");
	}
}

public partial class Levels : Node2D {
	private Node2D LevelParent {
		get {
			if (_levelParent is null) {
				//_levelParent = new Node2D() {
				//    Name = "_LevelsProxy"
				//};
				// HACK: `Levels` is an autoloaded global node, thus most
				//       things behave unexpectedly if added as a child.
				//       Avoid issues by creating the "level container"
				//       node within the current scene.
				//GetTree().CurrentScene.AddChild(_levelParent);
				_levelParent = this;
			}

			return _levelParent;
		}
	}
	private Node2D? _levelParent;

	public void TransitionToLevel(string toScene, NodePath entranceNodePath, Node2D exitNode) {
		// Get the instance of the new level. This also forces loading the new
		// scene, in case it hasn't yet been loaded.
		Level level = LoaderFor(toScene).GetLevel();
		AdjustLevelPositionRelativeToCurrent(level, entranceNodePath, exitNode);

		// Move the player to the new scene
		var player = this.Persistent().Player;
		player?.Reparent(level);

		// TODO: load adjacent scenes
		// TODO: unload no-longer-adjacent scenes
		// TODO: re-center the new scene (reposition adjacent scenes relative to new)
	}

	public static void AdjustLevelPositionRelativeToCurrent(Node2D level, NodePath entranceNodePath, Node2D exitNode) {
		var anchorNode = entranceNodePath.IsEmpty
			? null
			: level.GetNodeOrNull<Node2D>(entranceNodePath);
		var anchorOffset = anchorNode?.Position ?? Vector2.Zero;

		level.GlobalPosition = exitNode.GlobalPosition - anchorOffset;
	}

	private LevelLoader LoaderFor(string scene) {
		// Try to find a matching level loader from child nodes
		foreach (var child in LevelParent.GetChildren()) {
			if (child is not LevelLoader levelLoader) {
				continue;
			}

			if (levelLoader.LevelScene == scene) {
				return levelLoader;
			}
		}

		// Loader not found, create a new one
		return CreateLoader(scene);
	}

	private LevelLoader CreateLoader(string scene) {
		var loader = new LevelLoader {
			LevelScene = scene,
			Name = scene,
		};
		LevelParent.AddChild(loader);

		return loader;
	}
}
