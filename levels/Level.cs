using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


[Tool]
[GlobalClass]
public partial class Level : Node2D {
	[Signal]
	public delegate void PlayerEnteredEventHandler(Level previous);

	[Signal]
	public delegate void PlayerLeftEventHandler(Level enteredLevel);

	[Signal]
	public delegate void PlayerEnteredAdjacentEventHandler(Level level);

	[Signal]
	public delegate void PlayerLeftAdjacentEventHandler(Level level);

	public IEnumerable<LevelTransition> GetLevelTransitions() =>
		this.FindDescendantsOfType<LevelTransition>();

	public IEnumerable<Level> GetAdjacentLevels() =>
		GetLevelTransitions()
			.SelectMany(t => new[] { t.RedLevel, t.BlueLevel })
			.Where(t => t != this);

	public void Enter() {
		GD.Print($"Entering {Name}");

		var previousLevel = this.Persistent().CurrentLevel;
		this.Persistent().CurrentLevel = this;
		this.Persistent().Player?.Reparent(this);

		EmitSignal(SignalName.PlayerEntered, previousLevel);

		var previouslyAdjacentLevels = previousLevel?.GetAdjacentLevels() ?? [];
		var newAdjacentLevels = GetAdjacentLevels()
			.Where(other => !previouslyAdjacentLevels.Contains(other));
		var noLongerAdjacentLevels = previouslyAdjacentLevels
			.Where(other => other != this && !newAdjacentLevels.Contains(other));

		foreach (var other in newAdjacentLevels) {
			other.Show();
			EmitSignal(SignalName.PlayerEnteredAdjacent, this);
		}
		foreach (var other in noLongerAdjacentLevels) {
			other.Hide();
			EmitSignal(SignalName.PlayerLeftAdjacent, this);
		}

		GD.Print($"Newly adjacent:     {string.Join(", ", newAdjacentLevels.Select(l => l.Name))}");
		GD.Print($"No longer adjacent: {string.Join(", ", noLongerAdjacentLevels.Select(l => l.Name))}");
	}
}
