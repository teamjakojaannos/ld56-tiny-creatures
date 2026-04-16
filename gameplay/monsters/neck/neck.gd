@tool
extends Node2D

@export_tool_button("Swoop Left")
var debug_swoop_left = swoop_left
@export_tool_button("Swoop Right")
var debug_swoop_right = swoop_right
@export_tool_button("Unswoop Left")
var debug_unswoop_left = unswoop_left
@export_tool_button("Unswoop Right")
var debug_unswoop_right = unswoop_right
var _is_swooping_left: bool = false
var _is_swooping_right: bool = false

@onready var _swooper_left: Swooper = $LeftSwoop/LeftSwooper
@onready var _swooper_right: Swooper = $RightSwoop/RightSwooper
@onready var _hand_left: NeckHand = $HandLeft
@onready var _hand_right: NeckHand = $HandRight
@onready var _left_pivot: Node2D = $LeftSwoop
@onready var _right_pivot: Node2D = $RightSwoop
@onready var _killzone_left: Area2D = $LeftSwoop/Killzone
@onready var _killzone_right: Area2D = $RightSwoop/Killzone
@onready var _dangerzone: Area2D = $Dangerzone


func _ready() -> void:
	$Sprite.play("idle")


func _process(_delta: float) -> void:
	var player := Persistent.player
	if _dangerzone.overlaps_body(player) and not player.is_dead:
		_try_attack(player.global_position)


func swoop_left() -> bool:
	return await _swoop(_hand_left, _swooper_left, _killzone_left)


func swoop_right() -> bool:
	return await _swoop(_hand_right, _swooper_right, _killzone_right)


func unswoop_left() -> void:
	await _unswoop(_hand_left, _swooper_left)


func unswoop_right() -> void:
	await _unswoop(_hand_right, _swooper_right)


func _try_attack(target: Vector2) -> void:
	if _is_swooping_left or _is_swooping_right:
		return

	_is_swooping_left = true
	_is_swooping_right = true

	var left_offset := Vector2.LEFT * (6.0 + randf() * 16.0)
	var left_pos := target + left_offset
	_left_pivot.global_position = left_pos

	var right_offset := Vector2.RIGHT * (6.0 + randf() * 16.0)
	var right_pos := target + right_offset
	_right_pivot.global_position = right_pos

	get_tree().create_timer(0.1).timeout.connect(
		func():
			await _do_swoop(_hand_left, swoop_left, unswoop_left)
			_is_swooping_left = false
	)
	get_tree().create_timer(0.1).timeout.connect(
		func():
			await _do_swoop(_hand_right, swoop_right, unswoop_right)
			_is_swooping_right = false
	)


func _do_swoop(
		hand: NeckHand,
		swoop_fn: Callable,
		unswoop_fn: Callable,
) -> void:
	var is_hit = await swoop_fn.call()
	if is_hit:
		await _hit(hand)
	else:
		await get_tree().create_timer(0.5).timeout
		await unswoop_fn.call()


func _hit(hand: NeckHand) -> void:
	var pivot := hand.player_pivot

	Persistent.main_camera.detach_from_player()

	var player := Persistent.player
	player.reparent(pivot, false)
	player.position = Vector2.ZERO
	player.is_in_trouble = true
	player.die()

	get_tree().create_timer(3.0).timeout.connect(hand.disappear)
	# HACK: wait a moment for the animations to finish or tweeners will be in funky states
	await get_tree().create_timer(2.5).timeout

	Persistent.reset_player_to_hub()
	await Persistent.player_respawning

	Persistent.main_camera.attach_to_player()

	_hand_left.reset(false, false)
	_swooper_left.reset(false)
	_hand_right.reset(false, false)
	_swooper_right.reset(false)

	_is_swooping_left = false
	_is_swooping_right = false


func _swoop(hand: NeckHand, swooper: Swooper, killzone: Area2D) -> bool:
	hand.reset(false, false)
	swooper.reset(false)

	await hand.appear()

	swooper.swoop()

	await get_tree().create_timer(0.5).timeout
	await hand.grab()

	var player := Persistent.player
	return killzone.overlaps_body(player)


func _unswoop(hand: NeckHand, swooper: Swooper) -> void:
	hand.reset(true, true)
	swooper.reset(true)

	await hand.disappear()
