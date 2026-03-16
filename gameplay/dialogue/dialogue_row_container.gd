@tool
class_name DialogueRowContainer
extends HBoxContainer

@export var debug_dialogue_lines: Array[DialogueLineNew]
@export_group("Advanced settings")
@export var line_prefab: PackedScene = preload("uid://cp0nqrlg42f6s")

@export_group("Debug controls")
@export_tool_button("Next line")
var debug_next_button = _debug_next
@export_tool_button("Reset")
var debug_reset_button = _debug_reset

@onready var line_container: DialogueVBoxContainer = $Entries
@onready var portrait_container_left: Control = $PortraitsLeft
@onready var portrait_container_right: Control = $PortraitsRight


func _debug_reset() -> void:
	for child in line_container.get_children():
		child.queue_free()


func _debug_next() -> void:
	var idx = line_container.get_child_count()
	if idx >= debug_dialogue_lines.size():
		print("All lines already visible, early return out!")
		return

	var next_line = debug_dialogue_lines[idx]
	_append_line(next_line)


func _append_line(line: DialogueLineNew) -> void:
	var dialogue_row: DialogueRow = line_prefab.instantiate()

	var portrait_left: Node2D = null # TODO
	var portrait_right: Node2D = null # TODO

	line_container.add_child(dialogue_row)
	dialogue_row.offset_changed.connect(line_container.queue_sort)

	for child in line_container.get_children():
		if child is not DialogueRow:
			continue
		child.anim_offset = 64.0

	var delay = 0.0
	for child in line_container.get_children():
		if child is not DialogueRow:
			continue

		child.shift_up.call_deferred(delay)
		delay += 0.125

	dialogue_row.scroll_text.call_deferred(line.text)
