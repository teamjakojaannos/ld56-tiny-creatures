using System;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;


namespace Jakojaannos.WisperingWoods.Gameplay;

[Tool]
[GlobalClass]
public partial class StartsAtNode : Node {
	[Export]
	public Node2D? Spawnpoint { get; set; }

	[Export]
	[ExportGroup("Groups")]
	public Godot.Collections.Array<StringName> SpawnpointGroups { get; set; } = new();

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

		parent.Reparent(target.GetParent() ?? target);
		parent.GlobalPosition = target.GlobalPosition;

		// FIXME: Player.cs should be able to handle this by subbing to a signal
		if (parent is Player player) {
			player.Invulnerable = false;
			player.EmitSignal(Player.SignalName.ReadyToGo);
		}
	}
}
