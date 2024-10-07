using System;
using Godot;

public partial class Intro : Node2D {
	[Export]
	public DialogueTree? InitialDialogue;

	[Export]
	public DialogueTree? IntroDialogue;

	[Export]
	public DialogueTree? CageOpenDialogue;

	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public Node2D? WispInitialLocation;

	[Export]
	public AudioStreamPlayer? Tilulii;

	[Export]
	public AudioStreamPlayer? Beep;

	[Export]
	public CanvasLayer? ScreenFader;

	private Dialogue dialogue = null!;

	public override void _Ready() {
		base._Ready();

		dialogue = Dialogue.Instance(this);
	}

	public void Play() {
		ScreenFader!.Visible = true;
		AnimPlayer!.Play("fade_in");

		GetTree().CreateTimer(2.0f).Timeout += () => {
			dialogue.DialogueFinished += InitialDialogueFinished;

			this.MainCamera().ApplyCameraShake(30.0f, 30.0f);
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

	public void LanternOpen() {
		dialogue.DialogueFinished += LanternOpenDialogueDone;

		Tilulii?.Play();

		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			this.MainCamera().PositionSmoothingEnabled = true;
			this.MainCamera().PositionSmoothingSpeed = 2.5f;

			player.WispTarget = null;
			if (player.Wisp is RigidBody2D rigidBody) {
				rigidBody.ApplyImpulse(Vector2.Up * 10000.0f);
			}
			var playerSprite = GetNode<Node2D>("PlayerSprite");
			var wispLocation = player.Wisp.GlobalPosition;
			player.GlobalPosition = playerSprite.GlobalPosition;
			player.Wisp.GlobalPosition = wispLocation;

			playerSprite.Hide();
			player.setSpriteVisible(true);
		}
		GetTree().CreateTimer(2.0f).Timeout += () => {
			dialogue.StartDialogue(CageOpenDialogue!);
		};
	}

	private void LanternOpenDialogueDone() {
		dialogue.DialogueFinished -= LanternOpenDialogueDone;

		ReleasePlayer();
	}

	private void ReleasePlayer() {
		if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
			GetTree().CreateTimer(0.25f).Timeout += () => {
				player.setMovementEnabled(true);
			};

			GetTree().CreateTimer(2.0f).Timeout += () => {
				this.MainCamera().PositionSmoothingSpeed = 10.0f;

				GetTree().CreateTimer(1.0f).Timeout += () => {
					this.MainCamera().PositionSmoothingEnabled = false;
					this.MainCamera().Offset = Vector2.Zero;
					this.MainCamera().Position = Vector2.Zero;
				};
			};
		}
	}

	public void InitFadeIn() {
		ScreenFader!.Visible = true;
		ScreenFader!.GetNodeOrNull<ColorRect>("Color").Color = Colors.Black;
	}

	public void FadeToBlack() {
		ScreenFader!.Visible = true;
		ScreenFader!.GetNodeOrNull<ColorRect>("Color").Color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		AnimPlayer!.Play("fade_out");
	}

	public void FadeInAfterDeath() {
		InitFadeIn();
		AnimPlayer!.Play("fade_in");
		
		if (this.Persistent().Player is Player player) {
			player.LieDown();
			GetTree().CreateTimer(2.5f).Timeout += () => {
				player.GetUp();
			};
		}
	}
}
