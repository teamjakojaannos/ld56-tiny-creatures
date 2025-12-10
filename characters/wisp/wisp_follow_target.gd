@tool
class_name WispFollowTarget
extends Node2D

@export var follow_distance: float = 75.0
@export var draw_debug: bool = false:
	get:
		return draw_debug
	set(value):
		draw_debug = value
		queue_redraw()

var debug_radius: float = 10.0:
	get:
		return debug_radius
	set(value):
		debug_radius = value
		queue_redraw()
var _target_offset: Vector2 = Vector2.RIGHT * follow_distance

@onready var player: PlayerCharacter = get_parent()


func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint():
		return

	var input_dir: Vector2 = player.InputDirection
	if not input_dir.is_zero_approx():
		_target_offset = input_dir * follow_distance

	var target_pos = player.global_position + _target_offset
	global_position = global_position.lerp(target_pos, 5.0 * delta)


func _draw() -> void:
	if not draw_debug:
		return

	var color = Color.AQUA
	var fill_color = Color(color, 0.15)
	var outline_color = Color(color, 0.75)
	draw_circle(Vector2.ZERO, debug_radius, fill_color, true)
	draw_circle(Vector2.ZERO, debug_radius, outline_color, false)


func _get_configuration_warnings() -> PackedStringArray:
	if get_parent() is not PlayerCharacter:
		return ["Must be placed as a child of a PlayerCharacter!"]

	return []


func reset_idle_position(teleport: bool = false) -> void:
	_target_offset = Vector2.RIGHT * follow_distance

	if teleport:
		global_position = player.global_position + _target_offset
