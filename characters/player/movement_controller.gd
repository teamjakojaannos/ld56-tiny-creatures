@tool
class_name MovementController
extends Node

var input_direction: Vector2:
	get:
		return _input_direction
var is_allowed: bool = true
var _input_direction: Vector2 = Vector2.ZERO


func _ready() -> void:
	if Engine.is_editor_hint():
		set_process(false)


func _process(_delta: float) -> void:
	_input_direction = Vector2.ZERO

	if not is_allowed:
		return

	_input_direction = Input.get_vector("left", "right", "up", "down")
