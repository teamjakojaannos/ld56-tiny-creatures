@tool
class_name EditorOnly
extends Node


func _enter_tree() -> void:
	if not Engine.is_editor_hint():
		var target := get_parent()
		target.name = "EDITOR_ONLY"
		target.queue_free()
