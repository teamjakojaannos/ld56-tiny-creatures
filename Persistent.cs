using System.Linq;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

public static class PersistentExt {
	private static Persistent? s_instance;

	public static Persistent Persistent(this Node node) {
		return s_instance ??= node.GetTree().Root.GetNode<Persistent>("/root/Persistent");
	}
}

[Tool]
public partial class Persistent : Node2D {
	public static Persistent Instance(Node node) {
		return node.GetTree().Root.GetNode<Persistent>("Persistent");
	}

	[Export]
	[MustSetInEditor]
	[ExportGroup("Prewire")]
	public PlayerCharacter Player {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_player);
		set => this.SetExportProperty(ref _player, value);
	}
	private PlayerCharacter? _player;

	[Export]
	[MustSetInEditor]
	public Node2D PlayerController {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_playerController);
		set => this.SetExportProperty(ref _playerController, value);
	}
	private Node2D? _playerController;

	[Export]
	public int SavedCount { get; internal set; } = 0;

	[Signal]
	public delegate void WispSavedEventHandler(string location);

	[Signal]
	public delegate void PlayerRespawnedEventHandler();

	[Export]
	public Array<string> State = new();

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			// Workaround https://github.com/godotengine/godot/issues/71373
			// TL;DR: Persistent is an autoload, but when [Tool]-script gets
			//        autoloaded at editor startup, it ends up being created as
			//        a direct child of the editor window. The persistent scene
			//        contains the default player instance, which in turn has
			//        a camera as the child. This camera is then the first one
			//        in the hierarchy, taking priority over the default editor
			//        viewport.
			if (GetViewport() is Window) {
				// NOTE:
				// DO NOT FREE THE AUTOLOAD INSTANCE. That breaks the editor,
				// as e.g. Editor Settings seem to assume autoload instances
				// are never destroyed. Just removing the node from the tree is
				// enough to fix camera in-editor, while allowing the editor to
				// cling onto the instance.
				GetParent().RemoveChild(this);
			}
			return;
		}
	}

	public void ResetPlayerToHub() {
		var spawnpoint = (Node2D)GetTree()
			.GetNodesInGroup("HubSpawn")
			.PickRandom();

		//Intro.FadeToBlack();
		Player.IsInCinematic = true;
		Player.SetMovementEnabled(false);

		GetTree().CreateTimer(2.5f).Timeout += () => {
			Player.TeleportTo(spawnpoint);
			//Intro.FadeInAfterDeath();
		};
	}
}
