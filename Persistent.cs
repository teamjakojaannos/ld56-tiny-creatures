using System.Linq;
using Godot;
using Godot.Collections;

public static class PersistentExt {
	private static Persistent? _instance;

	public static Persistent Persistent(this Node node) {
		return _instance is not null
			? _instance
			: (_instance = node.GetTree().Root.GetNode<Persistent>("/root/Persistent"));
	}
}

public partial class Persistent : Node2D {
	public static Persistent Instance(Node node) {
		return node.GetTree().Root.GetNode<Persistent>("Persistent");
	}

	[Export]
	public Intro? Intro;

	[Export]
	public Player? Player;

	[Export]
	public int SavedCount { get; internal set; } = 0;

	[Signal]
	public delegate void WispSavedEventHandler(string location);

	[Signal]
	public delegate void PlayerRespawnedEventHandler();

	[Export]
	public Array<string> State = new();

	public override void _Ready() {
		var playIntro = false;
		foreach (var child in GetTree().Root.GetChildren()) {
			playIntro |= child.IsInGroup("PlayIntro");
		}

		if (playIntro) {
			Intro!.InitFadeIn();
			Player!.ReadyToGo += StartIntro;
		} else {
			Intro?.ScreenFader?.Hide();
			Intro?.Hide();
		}
	}

	private void StartIntro() {
		Player!.ReadyToGo -= StartIntro;

		Intro!.GetParentOrNull<Node2D>()?.RemoveChild(Intro);
		Player.GetParentOrNull<Node2D>()?.AddChild(Intro);
		Intro.GlobalPosition = Player.GlobalPosition;

		Player.SetupForIntro(Intro.WispInitialLocation!);
		Intro.Play();
	}

	public void ResetPlayerToHub() {
		var spawnpoint = (Node2D) GetTree()
			.GetNodesInGroup("HubSpawn")
			.PickRandom();

		Intro!.FadeToBlack();
		Player!.IsInCinematic = true;
		Player!.setMovementEnabled(false);

		GetTree().CreateTimer(2.5f).Timeout += () => {
			Player!.TeleportTo(spawnpoint);
			Intro.FadeInAfterDeath();
		};
	}
}
