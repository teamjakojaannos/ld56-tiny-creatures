using System.Linq;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

public static class PersistentExt {
	private static Persistent? _instance;

	public static Persistent Persistent(this Node node) {
		return _instance is not null
			? _instance
			: (_instance = node.GetTree().Root.GetNode<Persistent>("/root/Persistent"));
	}
}

[Tool]
public partial class Persistent : Node2D {
	public static Persistent Instance(Node node) {
		return node.GetTree().Root.GetNode<Persistent>("Persistent");
	}

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Intro Intro {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_intro);
		set => this.SetExportProperty(ref _intro, value);
	}
	public Intro? _intro;

	[Export]
	[MustSetInEditor]
	public Player Player {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_player);
		set => this.SetExportProperty(ref _player, value);
	}
	private Player? _player;

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
				QueueFree();
			}
			return;
		}

		var playIntro = false;
		foreach (var child in GetTree().Root.GetChildren()) {
			playIntro |= child.IsInGroup("PlayIntro");
		}

		if (playIntro) {
			Intro.InitFadeIn();
			Player.ReadyToGo += StartIntro;
		} else {
			Intro.ScreenFader?.Hide();
			Intro.Hide();
		}
	}

	private void StartIntro() {
		Player.ReadyToGo -= StartIntro;

		Intro.GetParentOrNull<Node2D>()?.RemoveChild(Intro);
		Player.GetParentOrNull<Node2D>()?.AddChild(Intro);
		Intro.GlobalPosition = Player.GlobalPosition;

		Player.SetupForIntro(Intro.WispInitialLocation!);
		Intro.Play();
	}

	public void ResetPlayerToHub() {
		var spawnpoint = (Node2D)GetTree()
			.GetNodesInGroup("HubSpawn")
			.PickRandom();

		Intro.FadeToBlack();
		Player.IsInCinematic = true;
		Player.SetMovementEnabled(false);

		GetTree().CreateTimer(2.5f).Timeout += () => {
			Player.TeleportTo(spawnpoint);
			Intro.FadeInAfterDeath();
		};
	}
}
