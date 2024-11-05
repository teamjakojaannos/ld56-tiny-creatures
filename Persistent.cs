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

	[Export(PropertyHint.File, "*.tscn")]
	public string? MainScenePath;

	[Export]
	public PackedScene? PlayerPrefab;

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

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("dbg_reset")) {
			Intro!.FadeToBlack();

			GetTree().CreateTimer(2.0f).Timeout += () => {
				CallDeferred(MethodName.Reset);
			};
		}
	}

	private void Reset() {
		if (MainScenePath is null || PlayerPrefab is null) {
			return;
		}

		Dialogue.Instance(this).CloseDialogue();
		SavedCount = 0;
		State = new();

		Player!.ReadyToGo -= StartIntro;
		Intro!.GetParentOrNull<Node2D>()?.RemoveChild(Intro);
		AddChild(Intro);
		Intro.AnimPlayer?.Play("RESET");
		var sprite = Intro?.GetNode<AnimatedSprite2D>("PlayerSprite");
		if (sprite is not null) {
			sprite.Visible = true;
			sprite.Animation = "IntroStandUpComedy";
			sprite.Frame = 0;
			sprite.Pause();
			sprite.Position = Vector2.Zero;
		}
		CameraControlExtension.Reset();

		GetTree().ChangeSceneToFile(MainScenePath);
		Player = PlayerPrefab.Instantiate<Player>();
		AddChild(Player);

		Intro!.InitFadeIn();
		Player!.ReadyToGo += StartIntro;

		GetTree().CreateTimer(1.0f).Timeout += () => {
			Intro!.InitFadeIn();
			Player.CallDeferred(Player.MethodName.Spawn);
		};
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
		var spawnpoint = (Node2D)GetTree()
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
