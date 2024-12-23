using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

/// <summary>
/// Executes nodes in sequence, stopping at the first one which has not yet
/// succeeded. Fails immediately if a child fails.
/// </summary>
public partial class BTSequence : BTControlFlow {
	public override StatusCode Tick() {
		var childNodes = GetChildren().OfType<BTNode>();

		// Iterate children until we find one which still hasn't succeeded, or
		// until we encounter one which fails.
		foreach (var child in childNodes) {
			var status = child.Tick();
			// If the child did not succeed (still running or failed), stop
			if (status != StatusCode.Success) {
				return status;
			}

			// Child succeeded, try next
		}

		// All children succeeded, sequence complete.
		return StatusCode.Success;
	}
}
