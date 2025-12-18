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
	public bool ShouldWaitForInitialScene = true;

	[Export]
	public Node2D? Spawnpoint { get; set; }

	[Export]
	[ExportGroup("Groups")]
	public Godot.Collections.Array<StringName> SpawnpointGroups { get; set; } = [];

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public override void _EnterTree() {
		if (Engine.IsEditorHint()) {
			return;
		}

		if (ShouldWaitForInitialScene) {
			var levelManager = GetTree().Root.GetNodeOrNull("LevelManager");
			levelManager.Connect(
				"initial_scene_ready",
				Callable.From(FindSpawnAndTeleport),
				(uint)ConnectFlags.OneShot
			);
		} else {
			Connect(
				SignalName.Ready,
				Callable.From(FindSpawnAndTeleport),
				(uint)ConnectFlags.OneShot
			);
		}
	}

	private void FindSpawnAndTeleport() {
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

		GD.PrintErr($"No spawns available for \"{GetPath()}\"");
		var parent = GetParentOrNull<PlayerCharacter>();
		if (parent is PlayerCharacter player) {
			player.RestoreCollision();
		}
	}

	private void TeleportToSpawn(Node2D target) {
		var nodeToTeleport = GetParent<Node2D>();
		if (!target.IsInsideTree() || target.IsQueuedForDeletion()) {
			throw new InvalidOperationException("Tried spawning at a non-existent/deleted node!");
		}

		nodeToTeleport.GetParent().RemoveChild(nodeToTeleport);
		target.AddSibling(nodeToTeleport, forceReadableName: true);

		nodeToTeleport.GlobalPosition = target.GlobalPosition;
		nodeToTeleport.ResetPhysicsInterpolation();
	}
}
