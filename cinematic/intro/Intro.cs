using Godot;

public partial class Intro : Node2D {
	[Export]
	public DialogueTree? InitialDialogue;

	[Export]
	public DialogueTree? IntroDialogue;

	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public Node2D? WispInitialLocation;

	private Dialogue dialogue = null!;

	public override void _Ready() {
		base._Ready();

		dialogue = Dialogue.Instance(this);
	}

	public void Play() {
		GetTree().CreateTimer(2.0f).Timeout += () => {
			dialogue.DialogueFinished += InitialDialogueFinished;
			dialogue.StartDialogue(InitialDialogue!);
		};
	}

	private void InitialDialogueFinished() {
		dialogue.DialogueFinished -= InitialDialogueFinished;

		AnimPlayer!.Play("01_stand_up");
	}

	public void StartIntroDialogue() {
		dialogue.DialogueFinished += IntroDialogueFinished;

		dialogue.StartDialogue(IntroDialogue!);
		var playerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
		playerSprite.Play("IdleDown");
	}

	private void IntroDialogueFinished() {
		dialogue.DialogueFinished -= IntroDialogueFinished;

		GetTree().CreateTimer(0.5f).Timeout += () => {
			AnimPlayer!.Play("02_walk_to_lantern");
		};
	}

	public void OpenLantern() {
		GetTree().CreateTimer(0.25f).Timeout += () => {
			AnimPlayer!.Play("03_open_lantern");
		};
	}

	private void ReleasePlayer() {
		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			var camera = player.GetNode<Camera2D>("Camera");
			camera.PositionSmoothingEnabled = true;
			camera.PositionSmoothingSpeed = 2.5f;

			var playerSprite = GetNode<Node2D>("PlayerSprite");
			var wispLocation = player.Wisp.GlobalPosition;
			player.GlobalPosition = playerSprite.GlobalPosition;
			player.Wisp.GlobalPosition = wispLocation;

			playerSprite.Hide();
			player.ReleaseAfterIntro();


			GetTree().CreateTimer(0.25f).Timeout += () => {
				player.WispTarget = null;
				if (player.Wisp is RigidBody2D rigidBody) {
					rigidBody.ApplyImpulse(Vector2.Up * 10000.0f);
				}

				player.setMovementEnabled(true);
			};

			GetTree().CreateTimer(2.0f).Timeout += () => {
				camera.PositionSmoothingSpeed = 10.0f;
			};
		}
	}
}
