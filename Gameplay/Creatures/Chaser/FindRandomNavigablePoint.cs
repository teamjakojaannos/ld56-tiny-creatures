using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

public partial class FindRandomNavigablePoint : BTNode {
	private RandomNumberGenerator _rng = new();

	public override StatusCode Tick(AIState state, float delta) {
		var target = state.GetState("target");
		if (target is not null) {
			return StatusCode.Success;
		}

		var newTarget = GetRandomNavigationPoint();
		if (newTarget is null) {
			return StatusCode.Failure;
		}

		state.SetState("target", newTarget);
		return StatusCode.Success;
	}

	private Vector2? GetRandomNavigationPoint() {
		var allPositions = GetTree()
			.GetNodesInGroup("MarkoMarkersRoot")
			.SelectMany(root => root.GetChildren().OfType<Marker2D>())
			.Select(marker => marker.GlobalPosition);

		if (_rng.TryPickRandom(allPositions, out var position)) {
			return position;
		}

		return null;
	}
}
