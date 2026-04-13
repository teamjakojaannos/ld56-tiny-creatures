extends Node2D

@export var skip_intro: bool = false

var _conv_01_initial: DialogueConversation = preload("uid://p8he4nqcn2cy")
var _conv_02_demands_help: DialogueConversation = preload("uid://sabahfm0papb")
var _conv_03_promise_to_help: DialogueConversation = preload("uid://bptgiby1p80cl")
var _conv_04_released: DialogueConversation = preload("uid://b7c02gqssw0vk")

@onready var main_camera: CameraManager = get_tree().get_first_node_in_group("MainCamera")
@onready var player_controller: PlayerController = Persistent.player_controller
@onready var player: Player = Persistent.player_controller.player
@onready var wisp: Wisp = Persistent.player_controller.wisp
@onready var animations: AnimationPlayer = $AnimationPlayer


func _ready() -> void:
	main_camera.set_fully_obscured()
	play.call_deferred()


func play() -> void:
	player_controller.movement.is_allowed = false
	player.rig.is_in_intro = true
	player.rig.is_prone = true
	player.rig.facing = Facing.Direction.RIGHT

	animations.active = true

	var wisp_initial_position: Node2D = $SpiritLantern/WispInitialLocation
	wisp.teleport(wisp_initial_position, true)

	# Wait a few seconds to make the intro start a bit less abrupt
	# HACK: this also gives time for the scene tree to stabilized after loading
	await _wait(2.0)

	main_camera.fade_to_visible(2.5)
	get_tree().create_timer(0.5).timeout.connect(
		func():
			main_camera.shake(30.0, 0.5, Vector2.DOWN * 0.8)
	)
	await main_camera.fade_finished

	if not skip_intro:
		await _wait(0.5)

		Conversation.begin(_conv_01_initial)
		await Conversation.finished

		# Dramatic pause, or sth
		# FIXME: do something to emphasize wisp
		# 1. make camera zoomed in at the start
		# 2. zoom out camera / pan wisp into view
		# etc.
		await _wait(1.0)

		Conversation.begin(_conv_02_demands_help)
		await Conversation.finished

		await _play_step("01_stand_up")

		Conversation.begin(_conv_03_promise_to_help)
		await Conversation.finished

		await _play_step("02_walk_to_lantern")
		await _play_step("03_open_lantern")
		$Tilulii.play()

	animations.active = false
	player.rig.is_in_intro = false
	player.rig.is_prone = false

	main_camera.camera.position_smoothing_enabled = true
	main_camera.camera.position_smoothing_speed = 1.0

	await _wisp_flies_loop_of_joy_around_the_player()

	if not skip_intro:
		Conversation.begin(_conv_04_released)
		await Conversation.finished

	player_controller.movement.is_allowed = true

	main_camera.camera.position_smoothing_speed = 10.0

	await _wait(1.0)
	main_camera.camera.position_smoothing_enabled = false
	main_camera.camera.offset = Vector2.ZERO
	main_camera.camera.position = Vector2.ZERO


func _wait(time_sec: float) -> void:
	await get_tree().create_timer(time_sec, false).timeout


func _play_step(anim_name: StringName) -> void:
	animations.play(anim_name)
	await animations.animation_finished


func _wisp_flies_loop_of_joy_around_the_player():
	# FIXME: this is really lazy, do this with paths or sth?
	# - it would be neat to be able to create a path and make wisp follow that
	# - ...which could be used for the other wisps
	await get_tree().create_timer(0.1).timeout
	wisp.clear_go_to_target()
	wisp.go_to(player.global_position + Vector2.DOWN * 48.0 + Vector2.LEFT * 64.0, true)
	await get_tree().create_timer(0.25).timeout
	wisp.go_to(player.global_position + Vector2.LEFT * 128.0, true)
	await get_tree().create_timer(0.25).timeout
	wisp.go_to(player.global_position + Vector2.UP * 128.0, true)
	await get_tree().create_timer(0.25).timeout
	wisp.go_to(player.global_position + Vector2.UP * 16 + Vector2.RIGHT * 48, true)
	await get_tree().create_timer(0.125).timeout
	wisp.clear_go_to_target()
