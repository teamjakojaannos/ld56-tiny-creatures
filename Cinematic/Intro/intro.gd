extends Node2D

@onready var main_camera: CameraManager = get_tree().get_first_node_in_group("MainCamera")
@onready var control_rig: PlayerController = Persistent.PlayerController
@onready var player: PlayerCharacter = Persistent.PlayerController.player
@onready var wisp: Wisp = Persistent.PlayerController.wisp
@onready var animations: AnimationPlayer = $AnimationPlayer


func _ready() -> void:
	_setup_scene_for_intro()

	await get_tree().create_timer(2.0).timeout
	play()


func play() -> void:
	main_camera.fade_to_visible()
	await main_camera.fade_finished

	main_camera.shake(30.0, 0.5, Vector2.DOWN * 0.8)
	await _wait(1.0)

	DialogueMan.ActiveDialogue = $InitialDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	# Dramatic pause, or sth
	# FIXME: do something to emphasize wisp
	# 1. make camera zoomed in at the start
	# 2. zoom out camera / pan wisp into view
	# etc.
	await _wait(1.0)

	DialogueMan.ActiveDialogue = $WispDemandsHelpDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	await _play_step("01_stand_up")

	DialogueMan.ActiveDialogue = $PlayerPromisesToHelpDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	await _play_step("02_walk_to_lantern")
	await _play_step("03_open_lantern")

	$Tilulii.play()
	main_camera.camera.position_smoothing_enabled = true
	main_camera.camera.position_smoothing_speed = 1.0

	$PlayerSprite.visible = false
	player.global_position = $PlayerSprite.global_position
	player.SpriteVisible = true

	await _wisp_flies_loop_of_joy_around_the_player()

	DialogueMan.ActiveDialogue = $WispReleasedDialogue
	DialogueMan.StartDialogue()
	await DialogueMan.DialogueFinished

	player.MovementEnabled = true
	main_camera.camera.position_smoothing_speed = 10.0

	await _wait(1.0)
	main_camera.camera.position_smoothing_enabled = false
	main_camera.camera.offset = Vector2.ZERO
	main_camera.camera.position = Vector2.ZERO


func _wait(time_sec: float) -> void:
	await get_tree().create_timer(time_sec, false).timeout


func _play_step(anim_name: StringName, wait: bool = true) -> void:
	animations.play(anim_name)
	animations.advance(0)

	# HACK: attempt to fix the animation playback track bug
	$PlayerSprite/AnimationPlayer.advance(0)
	$PlayerSprite/BaseAnimations.advance(0)

	if wait:
		await animations.animation_finished


func _setup_scene_for_intro():
	player.SpriteVisible = false
	player.MovementEnabled = false

	var wisp_initial_position: Node2D = $SpiritLantern/WispInitialLocation
	wisp.TeleportTo(wisp_initial_position.global_position, true)

	animations.play("01_stand_up")
	animations.advance(0)
	animations.stop(true)

	main_camera.set_fully_obscured()


func _wisp_flies_loop_of_joy_around_the_player():
	# FIXME: this is really lazy, must be some better way to do the victory loop-da-loop?
	await get_tree().create_timer(0.1).timeout
	wisp.Release()
	wisp.GoToSync(player.global_position + Vector2.DOWN * 32.0 + Vector2.LEFT * 64.0)
	await get_tree().create_timer(0.25).timeout
	wisp.GoToSync(player.global_position + Vector2.LEFT * 128.0)
	await get_tree().create_timer(0.25).timeout
	wisp.GoToSync(player.global_position + Vector2.UP * 48.0)
	await get_tree().create_timer(0.125).timeout
	wisp.GoToSync(player.global_position + Vector2.UP * 16 + Vector2.RIGHT * 48)
	await get_tree().create_timer(0.125).timeout
	wisp.Release()
