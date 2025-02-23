using System;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;


namespace Jakojaannos.WisperingWoods.Gameplay;

[Tool]
[GlobalClass]
[RequireParent(typeof(Node2D))]
public partial class StartsAtNode : Node {
	[Export]
	public Node2D? Spawnpoint { get; set; }

	[Export]
	[ExportGroup("Groups")]
	public Godot.Collections.Array<StringName> SpawnpointGroups { get; set; } = [];

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		if (Spawnpoint is not null) {
			CallDeferred(MethodName.TeleportToSpawn, Spawnpoint);
			return;
		}

		foreach (var spawnpointGroup in SpawnpointGroups) {
			var spawns = GetTree().GetNodesInGroup(spawnpointGroup);
			if (spawns.Count == 0) {
				continue;
			}

			if (spawns.PickRandom() is Node2D spawnpointFromGroup) {
				CallDeferred(MethodName.TeleportToSpawn, spawnpointFromGroup);
				return;
			}
		}

		GD.PrintErr("No spawns available");
	}

	private void TeleportToSpawn(Node2D target) {
		var parent = GetParent<Node2D>();
		if (!target.IsInsideTree() || target.IsQueuedForDeletion()) {
			throw new InvalidOperationException("Tried spawning at a non-existent/deleted node!");
		}

		// FIXME: Player.cs should be able to handle this by subbing to a signal
		if (parent is PlayerCharacter player) {
			player.TeleportTo(target);
		} else {
			parent.Reparent(target.GetParent() ?? target);
			parent.GlobalPosition = target.GlobalPosition;
			parent.ResetPhysicsInterpolation();
		}
	}
}
