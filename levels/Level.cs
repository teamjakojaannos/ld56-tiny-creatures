using System.Collections.Generic;
using Godot;

public static class NodeExt {
	public static IEnumerable<T> FindDescendantsOfType<T>(this Node node) {
		var descendants = new List<T>();
		foreach (var child in node.GetChildren()) {
			if (child is T matching) {
				descendants.Add(matching);
			}

			var transitiveDescendants = child.FindDescendantsOfType<T>();
			descendants.AddRange(transitiveDescendants);
		}

		return descendants;
	}
}

[Tool]
[GlobalClass]
public partial class Level : Node2D {
	[Export]
	public Godot.Collections.Array<LevelTransition> LevelTransitions {
		get {
			return new(this.FindDescendantsOfType<LevelTransition>());
		}
		set {
			// HACK: NOOP
		}
	}
}
