using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.World;

public partial class Levels : Node2D {
	public Node2D LevelParent => this;

	private Dictionary<string, uint> _levelReferenceCounter = [];
	private string? _currentScene;

	public override void _Ready() {
		if (!Engine.IsEditorHint()) {
			CallDeferred(MethodName.KidnapInitialScene);
		}
	}
	private void KidnapInitialScene() {
		var initialScene = GetTree().CurrentScene;
		if (initialScene is not Level initialLevel) {
			GD.PrintErr("Initial scene is not a level. Skipping kidnapping to LevelLoader.");
			return;
		}

		var initialSceneResource = initialLevel.SceneFilePath;

		GD.Print($"Kidapping initial scene (\"{initialLevel.Name}\", resource: {initialSceneResource}). Reparenting under {LevelParent.GetPath()}");
		_currentScene = initialSceneResource;

		var kidnapper = CreateLoader(_currentScene);
		kidnapper.WithExistingInstance(initialLevel);
		initialLevel.Reparent(kidnapper);

		ShowLevel(_currentScene);
		RefreshLevelReferences(initialLevel);
	}

	public void TransitionToLevel(string toScene, NodePath entranceNodePath, Node2D exitNode) {
		// Get the instance of the new level. This also forces loading the new
		// scene, in case it hasn't yet been loaded.
		var nextLevel = LoaderFor(toScene).GetLevel();
		AdjustLevelPositionRelativeToCurrent(nextLevel, entranceNodePath, exitNode);

		// Move the player to the new scene
		var player = this.Persistent().Player;
		player.Reparent(nextLevel);
		player.ResetPhysicsInterpolation();

		if (_currentScene is null) {
			throw new NotImplementedException("TODO: hijack the initial scene");
		}

		// FIXME: this might be too naive for 1-way transitions
		//	- if transition is intentionally 1-way (toScene does not have a
		//    transition back to _currentScene) the previous scene is
		//    immediately hidden. This likely occurs on-screen, unless the
		//    transition area is set up far inside the new scene.
		//  - For now, the transitions should be set up deep inside new scene
		//    in case 1-way transitions are needed, but it would be more robust
		//    to scan the previous scene for transitions before decrementing
		//    the refCount.
		HideLevel(_currentScene);
		ShowLevel(toScene);

		RefreshLevelReferences(nextLevel);

		_currentScene = toScene;

		// TODO: re-center the new scene (reposition adjacent scenes relative to new)
	}

	public static void AdjustLevelPositionRelativeToCurrent(Node2D level, NodePath entranceNodePath, Node2D exitNode) {
		var anchorNode = entranceNodePath.IsEmpty
			? null
			: level.GetNodeOrNull<Node2D>(entranceNodePath);
		var anchorOffset = anchorNode?.Position ?? Vector2.Zero;

		level.GlobalPosition = exitNode.GlobalPosition - anchorOffset;
		level.ResetPhysicsInterpolation();
	}

	public void PreloadLevel(string sceneName, NodePath entranceNodePath, Node2D transitionNode) {
		var loader = LoaderFor(sceneName);
		loader.ShowLevel(entranceNodePath, transitionNode);
	}

	public void FreeLevel(string sceneName) {
		var loader = LoaderFor(sceneName);
		loader.HideLevel();
	}

	private void RefreshLevelReferences(Level newLevel) {
		var unloaded = new List<string>();
		foreach (var (level, refCount) in _levelReferenceCounter) {
			if (level is null) {
				throw new NullReferenceException("Level scene name/id cannot be null.");
			}

			if (refCount == 0) {
				GD.Print($"Freeing level \"{level}\"");
				FreeLevel(level);
				unloaded.Add(level);
			} else {
				GD.Print($"Preloading level \"{level}\"");

				var transition = newLevel
					.GetLevelTransitions()
					.Where(t => t.OtherScene == level)
					.FirstOrDefault();

				if (transition is not null) {
					PreloadLevel(level, transition.EntranceNodePath, transition);
				}
			}
		}

		unloaded.ForEach(level => {
			GD.Print($"Removing reference counter of an unloaded level \"{level}\"");
			_levelReferenceCounter.Remove(level);
		});
	}

	private void HideLevel(string level) {
		if (_levelReferenceCounter.ContainsKey(level)) {
			_levelReferenceCounter[level] -= 1;

			var adjacent = LoaderFor(level).GetLevel().GetLevelTransitions();
			foreach (var transition in adjacent) {
				var other = transition.OtherScene;
				_levelReferenceCounter[other] -= 1;
			}
		} else {
			throw new InvalidOperationException($"Cannot decrement reference count of an unloaded level \"{level}\"!");
		}
	}

	private void ShowLevel(string level) {
		_levelReferenceCounter[level] = _levelReferenceCounter.GetValueOrDefault(level) + 1;

		var adjacent = LoaderFor(level).GetLevel().GetLevelTransitions();
		foreach (var transition in adjacent) {
			var other = transition.OtherScene;
			_levelReferenceCounter[other] = _levelReferenceCounter.GetValueOrDefault(other) + 1;
		}
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
