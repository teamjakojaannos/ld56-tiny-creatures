@tool
class_name RemarkBubble
extends Node2D

@export var text: String:
	get:
		return text
	set(value):
		text = value
		if label:
			label.text = value

		_refresh.call_deferred()

@onready var label: Label = $UI/Container/MarginContainer/Label
@onready var container: Container = $UI/Container


func show_remark(
		new_text: String,
		duration: float = 2,
) -> void:
	visible = true
	text = new_text

	await get_tree().create_timer(duration, false).timeout
	visible = false


func _refresh() -> void:
	if not label:
		return

	label.text = text
