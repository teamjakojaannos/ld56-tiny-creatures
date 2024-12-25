using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

/// <summary>
/// Executes child nodes in order, stopping at the first which does not fail.
///
/// Sometimes called a "Fallback" node.
/// </summary>
[Tool]
[GlobalClass]
public partial class BTSelector : BTControlFlow {
	public override StatusCode Tick(AIState state, float delta) {
		var childNodes = GetChildren().OfType<BTNode>();

		// Iterate child nodes until the first one which does not report failure.
		foreach (var child in childNodes) {
			var status = child.Tick(state, delta);
			if (status != StatusCode.Failure) {
				// Child is running or completed successfully, nothing else to do.
				return status;
			}

			// Child reported failure, try next.
		}

		// All children returned a failure status
		return StatusCode.Failure;
	}
}
