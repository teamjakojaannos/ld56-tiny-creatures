extends Node2D

@onready var main_camera: CameraControl = get_tree().get_first_node_in_group("MainCamera")
@onready var control_rig: PlayerController = Persistent.PlayerController
@onready var player: PlayerCharacter = Persistent.PlayerController.player
@onready var wisp: WispCharacter = Persistent.PlayerController.wisp

func _ready() -> void:
	_setup_scene_for_intro();

	await get_tree().create_timer(2.0).timeout
	Play()

func _setup_scene_for_intro():
	player.SpriteVisible = false
	player.MovementEnabled = false

	var wisp_initial_position: Node2D = $SpiritLantern/WispInitialLocation
	wisp.TeleportTo(wisp_initial_position.global_position, true)

	$AnimationPlayer.play("RESET")

	main_camera.SetFullyObscured()

func Play() -> void:
	main_camera.FadeToVisible()
	await main_camera.FadeFinished

	# FIXME: separate h/v shake and apply shake downwards to indicate the fall
	main_camera.ApplyCameraShake(30.0, 30.0)
	await get_tree().create_timer(1.0).timeout

	DialogueMan.ActiveDialogue = $InitialDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	# Dramatic pause, or sth
	# FIXME: do something to emphasize wisp
	# 1. make camera zoomed in at the start
	# 2. zoom out camera / pan wisp into view
	# etc.
	await get_tree().create_timer(1.0).timeout

	DialogueMan.ActiveDialogue = $WispDemandsHelpDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	$AnimationPlayer.play("01_stand_up")
	await $AnimationPlayer.animation_finished

	DialogueMan.ActiveDialogue = $PlayerPromisesToHelpDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	$AnimationPlayer.play("02_walk_to_lantern")
	await $AnimationPlayer.animation_finished

	$AnimationPlayer.play("03_open_lantern")
	await $AnimationPlayer.animation_finished

	await _wisp_flies_loop_of_joy_around_the_player()

	DialogueMan.ActiveDialogue = $WispReleasedDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	$PlayerSprite.visible = false
	player.SpriteVisible = true
	player.MovementEnabled = true

func _wisp_flies_loop_of_joy_around_the_player():
	# FIXME: this is really lazy, must be some better way to do the victory loop-da-loop?
	await get_tree().create_timer(0.1).timeout
	wisp.Release()
	wisp.GoToSync(player.global_position + Vector2.DOWN * 32.0)
	await get_tree().create_timer(0.25).timeout
	wisp.GoToSync(player.global_position + Vector2.LEFT * 72.0)
	await get_tree().create_timer(0.25).timeout
	wisp.GoToSync(player.global_position + Vector2.UP * 64.0)
	await get_tree().create_timer(0.125).timeout
	wisp.Release()

#public void Play() {
#		ScreenFader!.Visible = true;
#		AnimPlayer!.Play("fade_in");
#
#		GetTree().CreateTimer(2.0f).Timeout += () => {
#			dialogue.DialogueFinished += InitialDialogueFinished;
#
#			this.main_camera().ApplyCameraShake(30.0f, 30.0f);
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
#			this.main_camera().PositionSmoothingEnabled = true;
#			this.main_camera().PositionSmoothingSpeed = 2.5f;
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
#				this.main_camera().PositionSmoothingSpeed = 10.0f;
#
#				GetTree().CreateTimer(1.0f).Timeout += () => {
#					this.main_camera().PositionSmoothingEnabled = false;
#					this.main_camera().Offset = Vector2.Zero;
#					this.main_camera().Position = Vector2.Zero;
#				};
#			};
#		}
#	}
