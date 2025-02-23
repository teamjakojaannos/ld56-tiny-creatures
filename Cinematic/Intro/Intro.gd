extends Node2D

@onready var MainCamera: CameraControl = get_tree().get_first_node_in_group("MainCamera")

func _ready() -> void:
	print("Intro delay")
	await get_tree().create_timer(2.0).timeout

	print("Intro start")
	Play()

func Play() -> void:
	MainCamera.SetFullyObscured()
	MainCamera.FadeToVisible()
	await MainCamera.FadeFinished

	# FIXME: separate h/v shake and apply shake downwards to indicate the fall
	MainCamera.ApplyCameraShake(30.0, 30.0)
	await get_tree().create_timer(1.0).timeout

	DialogueMan.ActiveDialogue = $InitialDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished
	
	$AnimationPlayer.play("01_stand_up")
	await $AnimationPlayer.animation_finished

	DialogueMan.ActiveDialogue = $WispDemandsHelpDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	$AnimationPlayer.play("02_walk_to_lantern")
	await $AnimationPlayer.animation_finished
	
	$AnimationPlayer.play("03_open_lantern")
	await $AnimationPlayer.animation_finished

#public void Play() {
#		ScreenFader!.Visible = true;
#		AnimPlayer!.Play("fade_in");
#
#		GetTree().CreateTimer(2.0f).Timeout += () => {
#			dialogue.DialogueFinished += InitialDialogueFinished;
#
#			this.MainCamera().ApplyCameraShake(30.0f, 30.0f);
#			dialogue.StartDialogue(InitialDialogue!);
#		};
#	}
#
#	private void InitialDialogueFinished() {
#		dialogue.DialogueFinished -= InitialDialogueFinished;
#
#		AnimPlayer!.Play("01_stand_up");
#	}
#
#	public void StartIntroDialogue() {
#		dialogue.DialogueFinished += IntroDialogueFinished;
#
#		dialogue.StartDialogue(IntroDialogue!);
#		var playerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
#		playerSprite.Play("IdleDown");
#	}
#
#	private void IntroDialogueFinished() {
#		dialogue.DialogueFinished -= IntroDialogueFinished;
#
#		GetTree().CreateTimer(0.5f).Timeout += () => {
#			AnimPlayer!.Play("02_walk_to_lantern");
#		};
#	}
#
#	public void OpenLantern() {
#		GetTree().CreateTimer(0.25f).Timeout += () => {
#			AnimPlayer!.Play("03_open_lantern");
#		};
#	}
#
#	public void LanternOpen() {
#		dialogue.DialogueFinished += LanternOpenDialogueDone;
#
#		Tilulii?.Play();
#
#		if (GetTree().GetFirstNodeInGroup("Player") is PlayerCharacter player) {
#			this.MainCamera().PositionSmoothingEnabled = true;
#			this.MainCamera().PositionSmoothingSpeed = 2.5f;
#
#			player.WispTarget = null;
#			if (player.Wisp is RigidBody2D rigidBody) {
#				rigidBody.ApplyImpulse(Vector2.Up * 10000.0f);
#			}
#			var playerSprite = GetNode<Node2D>("PlayerSprite");
#			var wispLocation = player.Wisp.GlobalPosition;
#			player.GlobalPosition = playerSprite.GlobalPosition;
#			player.Wisp.GlobalPosition = wispLocation;
#
#			playerSprite.Hide();
#			player.SetSpriteVisible(true);
#		}
#		GetTree().CreateTimer(2.0f).Timeout += () => {
#			dialogue.StartDialogue(CageOpenDialogue!);
#		};
#	}
#
#	private void LanternOpenDialogueDone() {
#		dialogue.DialogueFinished -= LanternOpenDialogueDone;
#
#		ReleasePlayer();
#	}
#
#	private void ReleasePlayer() {
#		if (GetTree().GetFirstNodeInGroup("Player") is PlayerCharacter player) {
#			GetTree().CreateTimer(0.25f).Timeout += () => {
#				player.SetMovementEnabled(true);
#			};
#
#			GetTree().CreateTimer(2.0f).Timeout += () => {
#				this.MainCamera().PositionSmoothingSpeed = 10.0f;
#
#				GetTree().CreateTimer(1.0f).Timeout += () => {
#					this.MainCamera().PositionSmoothingEnabled = false;
#					this.MainCamera().Offset = Vector2.Zero;
#					this.MainCamera().Position = Vector2.Zero;
#				};
#			};
#		}
#	}
