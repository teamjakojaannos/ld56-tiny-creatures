@tool
class_name DialogueRowContainer
extends HBoxContainer

@export var debug_dialogue_lines: Array[DialogueLineNew]
@export var line_prefab: PackedScene = preload("uid://cp0nqrlg42f6s")
@export var portrait_prefab: PackedScene = preload("uid://di32ud1oobxw8")

@export_group("Debug controls")
@export_tool_button("Next line")
var debug_next_button = _debug_next
@export_tool_button("Reset")
var debug_reset_button = _debug_reset
var _previous_line: DialogueLineNew

@onready var line_container: DialogueVBoxContainer = $Entries
@onready var portrait_container_left: DialogueVBoxContainer = $PortraitsLeft
@onready var portrait_container_right: Control = $PortraitsRight


func _debug_reset() -> void:
	for child in line_container.get_children():
		child.queue_free()

	for child in portrait_container_left.get_children():
		child.queue_free()

	for child in portrait_container_right.get_children():
		child.queue_free()

	_previous_line = null


func _debug_next() -> void:
	var idx = line_container.get_child_count()
	if idx >= debug_dialogue_lines.size():
		var delay = 0.0
		for child_idx in line_container.get_child_count():
			line_container.get_child(child_idx).yeet(delay)
			portrait_container_left.get_child(child_idx).yeet(delay)
			portrait_container_right.get_child(child_idx).yeet(delay)
			delay += 0.1
		return

	var next_line = debug_dialogue_lines[idx]
	_append_line(next_line)


func _append_line(line: DialogueLineNew) -> void:
	var dialogue_row: DialogueRow = line_prefab.instantiate()

	var portrait_left: DialoguePortrait = portrait_prefab.instantiate()
	var portrait_right: DialoguePortrait = portrait_prefab.instantiate()

	var is_chain := _previous_line and _previous_line.speaker == line.speaker
	_previous_line = line

	var is_empty = line.text.length() == 0

	if is_chain or is_empty:
		portrait_right.modulate = Color.TRANSPARENT
		portrait_left.modulate = Color.TRANSPARENT

		if is_empty:
			dialogue_row.modulate = Color.TRANSPARENT
	elif line.side == DialogueLineNew.Side.LEFT:
		portrait_right.modulate = Color.TRANSPARENT
	elif line.side == DialogueLineNew.Side.RIGHT:
		portrait_left.modulate = Color.TRANSPARENT

	line_container.add_child(dialogue_row)
	dialogue_row.offset_changed.connect(line_container.queue_sort)

	portrait_container_left.add_child(portrait_left)
	portrait_left.speaker = line.speaker
	portrait_left.offset_changed.connect(portrait_container_left.queue_sort)

	portrait_container_right.add_child(portrait_right)
	portrait_right.offset_changed.connect(portrait_container_right.queue_sort)
	portrait_right.speaker = line.speaker

	for child in line_container.get_children():
		child.anim_offset = 64.0

	for child in portrait_container_left.get_children():
		child.anim_offset = 64.0
	for child in portrait_container_right.get_children():
		child.anim_offset = 64.0

	var delay = 0.0
	for child in line_container.get_children():
		child.shift_up.call_deferred(delay)
		delay += 0.1

	delay = 0.0
	for child in portrait_container_left.get_children():
		child.shift_up.call_deferred(delay)
		delay += 0.125

	delay = 0.0
	for child in portrait_container_right.get_children():
		child.shift_up.call_deferred(delay)
		delay += 0.125

	await get_tree().create_timer(0.25).timeout
	dialogue_row.scroll_text.call_deferred(line.text)
