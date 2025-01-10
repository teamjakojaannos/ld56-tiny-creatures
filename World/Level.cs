using System.Collections.Generic;
using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.World;

[Tool]
[GlobalClass]
public partial class Level : Node2D {
	public IEnumerable<LevelTransition> GetLevelTransitions() {
		return this.RecursiveFindAdjacentLevels();
	}
}

internal static class RecursiveFindAdjacentLevelsExt {
	public static IEnumerable<LevelTransition> RecursiveFindAdjacentLevels(this Node2D node) {
		return node.GetChildren()
			.OfType<Node2D>()
			.SelectMany(RecursiveFindAdjacentLevels)
			.Concat(
				node.GetChildren()
					.OfType<LevelTransition>()
			);
	}
}
