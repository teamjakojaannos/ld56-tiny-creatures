using System;

using Godot;

namespace Jakojaannos.WisperingWoods.World;

public partial class LevelLoader : Node2D {
	public string? LevelScene { get; set; }

	private State _state = new Unloaded();
	private State CurrentState {
		get => _state;
		set {
			_state = value;
			ProcessMode = _state.ProcessMode;
		}
	}

	public override void _Ready() {
		base._Ready();
		CurrentState = new Unloaded();
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (LevelScene is null) {
			return;
		}

		if (CurrentState is Requested requested && requested.IsReady) {
			CurrentState = new Loaded(LevelScene);
		}

		if (CurrentState is Loaded loaded && loaded.IsReady) {
			CurrentState = new Instantiated(loaded.GetLevel(), this);
		}
	}

	public Level GetLevel() {
		GD.Print($"Getting level \"{LevelScene}\" ({CurrentState.GetType().Name})");
		if (LevelScene is null) {
			throw new InvalidOperationException("Level scene is not configured!");
		}

		// Level has not been requested => load and instantiate immediately
		if (CurrentState is Unloaded || CurrentState is Requested) {
			var isNotRequestedYet = ResourceLoader.LoadThreadedGetStatus(LevelScene) == ResourceLoader.ThreadLoadStatus.InvalidResource;
			GD.Print($"Loading (threaded: {!isNotRequestedYet})");
			var resource =
				isNotRequestedYet
					// Not requested yet => load ASAP on main thread
					? ResourceLoader.Load(LevelScene)
					// Is already being loaded => wait until finished
					: ResourceLoader.LoadThreadedGet(LevelScene);
			if (resource is not PackedScene scene) {
				throw new InvalidOperationException("Level scene path does not point to a scene resource!");
			}

			GD.Print("Instantiating");

			// Player should already be transitioning to the level but it has
			// not yet been instantiated. This is somewhat awkward, so just
			// block the main thread to instantiate the scene ASAP.
			var level = scene.Instantiate<Level>();
			CurrentState = new Instantiated(level, this);

			GD.Print("Instantiating done!");

			return level;
		}
		// Level is loaded, but still being instantiated => block until fully instantiated
		else if (CurrentState is Loaded loaded) {
			var level = loaded.GetLevel();
			CurrentState = new Instantiated(level, this);

			return level;
		}
		// Level instance has already been loaded and instantiated
		else if (CurrentState is Instantiated instantiated) {
			return instantiated.Level;
		}
		// Unexpected state
		else {
			throw new InvalidOperationException($"LevelLoader is in an unexpected state: {CurrentState.GetType().Name}");
		}
	}

	public void ShowLevel() {
		if (LevelScene is null) {
			return;
		}

		if (CurrentState is Unloaded) {
			CurrentState = new Requested(LevelScene);
		}
	}

	public void HideLevel() {
		CurrentState = new Unloaded();
	}

	private abstract class State {
		public abstract ProcessModeEnum ProcessMode { get; }
	}

	private sealed class Unloaded : State {
		public override ProcessModeEnum ProcessMode => ProcessModeEnum.Disabled;
	}

	private sealed class Requested : State {
		public string Path { init; get; }

		public override ProcessModeEnum ProcessMode => ProcessModeEnum.Always;

		public bool IsReady => ResourceLoader.LoadThreadedGetStatus(Path) == ResourceLoader.ThreadLoadStatus.Loaded;

		public Requested(string path) {
			Path = path;
		}
	}

	private sealed class Loaded : State {
		public override ProcessModeEnum ProcessMode => ProcessModeEnum.Always;

		public PackedScene Scene { init; get; }

		public bool IsReady => level != null || WorkerThreadPool.IsTaskCompleted(taskId);

		private Level? level;
		private long taskId;

		public Loaded(string path) {
			var loaded = ResourceLoader.LoadThreadedGet(path);
			if (loaded is not PackedScene scene) {
				throw new InvalidOperationException("Loaded level resource is not a scene!");
			}

			Scene = scene;
			taskId = WorkerThreadPool.AddTask(
				Callable.From(() => level = Scene.Instantiate<Level>()),
				highPriority: true,
				description: "Instantiate the loaded level"
			);
		}

		public Level GetLevel() {
			WorkerThreadPool.WaitForTaskCompletion(taskId);
			if (level is null) {
				throw new InvalidOperationException("Unexpected failure while instantiating a level!");
			}

			return level;
		}
	}

	private sealed class Instantiated : State {
		public Level Level { init; get; }

		public override ProcessModeEnum ProcessMode => ProcessModeEnum.Disabled;

		public Instantiated(Level instance, Node2D parent) {
			Level = instance;
			// Explicitly set process mode of the level root to allow the level
			// to process, even if it is inside a disabled loader node.

			Level.ProcessMode = ProcessModeEnum.Pausable;
			parent.AddChild(Level);
		}
	}
}

