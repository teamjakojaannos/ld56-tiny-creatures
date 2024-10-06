using Godot;

public partial class Persistent : Node2D {
	[Export]
	public Intro? Intro;

	[Export]
	public Player? Player;

	public override void _Ready() {
		var playIntro = false;
		foreach (var child in GetTree().Root.GetChildren()) {
			playIntro |= child.IsInGroup("PlayIntro");
		}

		if (playIntro) {
			Player!.ReadyToGo += StartIntro;
		} else {
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
}
