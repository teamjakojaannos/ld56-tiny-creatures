@tool
class_name Shaker
extends Node2D

@export var target: Node2D
@export var target_property: StringName = "position"
@export var magnitude: float = 30.0
@export var shake_direction: Vector2 = Vector2.ZERO
@export_range(0.0, 1.0, 0.01) var strength: float = 0.0:
	get:
		return strength
	set(value):
		strength = value

		if value > 0:
			set_process(true)
		else:
			#set_process(false)
			_target_offset = Vector2.ZERO

var _target_offset: Vector2:
	get:
		if not target:
			return Vector2.ZERO

		var value = target.get(target_property) as Vector2
		if not value:
			return Vector2.ZERO

		return value
	set(value):
		if not target:
			return Vector2.ZERO

		target.set(target_property, value)


func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		return

	var amount = magnitude * strength

	# apply reduced shake on the minor axis, to strengthen the direction effect
	var ratio_x = max(0, abs(shake_direction.y) - abs(shake_direction.x))
	var ratio_y = max(0, abs(shake_direction.x) - abs(shake_direction.y))
	var amount_x = amount * lerpf(1.0, 0.0, ratio_x)
	var amount_y = amount * lerpf(1.0, 0.0, ratio_y)

	var dir_offset = shake_direction * amount
	var new_offset = dir_offset + Vector2(
		randf_range(-amount_x, amount_x),
		randf_range(-amount_y, amount_y),
	)
	_target_offset = new_offset
