@tool
class_name DialogueRowContainer
extends HBoxContainer

@export var line_prefab: PackedScene = preload("uid://cp0nqrlg42f6s")
@export var choice_line_prefab: PackedScene = preload("uid://cwtj7o7amvxjl")
@export var portrait_prefab: PackedScene = preload("uid://di32ud1oobxw8")

var is_playing_animation: bool:
	get:
		return _cooldown or _cooldown2
var active_choice: DialogueChoiceRow:
	get:
		var count := line_container.get_child_count()
		if count == 0:
			return null

		var last_child = line_container.get_child(count - 1)
		if last_child is not DialogueChoiceRow:
			return null

		return last_child
var _cooldown: bool = false
var _cooldown2: bool = false
var _previous_speaker: DialogueSpeaker

@onready var line_container: DialogueVBoxContainer = $Entries
@onready var portrait_container_left: DialogueVBoxContainer = $PortraitsLeft
@onready var portrait_container_right: Control = $PortraitsRight


func highlight_option(option: int) -> void:
	var choice := active_choice
	if is_instance_valid(choice):
		choice.highlight_option(option)


func next(
		text: String,
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
) -> void:
	await _append_line(text, speaker, side)


func next_choice(
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
		a: String,
		b: String,
		c: String,
) -> void:
	_append_choice(speaker, side, a, b, c)


func reset() -> void:
	_cooldown = false
	_cooldown2 = false

	for child in line_container.get_children():
		child.queue_free()

	for child in portrait_container_left.get_children():
		child.queue_free()

	for child in portrait_container_right.get_children():
		child.queue_free()

	_previous_speaker = null


func end_dialogue() -> void:
	_cooldown2 = true
	var total_lines := line_container.get_child_count()
	var delay := 0.0
	for child_idx in line_container.get_child_count():
		# Only delay the last 4 lines as rest are already invisible.
		# Otherwise the end animation speed would depend on the number of
		# lines in the dialogue.
		if child_idx >= total_lines - 4:
			line_container.get_child(child_idx).yeet(delay, 1.0)
			portrait_container_left.get_child(child_idx).yeet(delay, 1.25)
			portrait_container_right.get_child(child_idx).yeet(delay, 1.25)
			delay += 0.1
		else:
			line_container.get_child(child_idx).queue_free()
			portrait_container_left.get_child(child_idx).queue_free()
			portrait_container_right.get_child(child_idx).queue_free()

	var t := 1.25 + delay
	get_tree().create_timer(t).timeout.connect(
		func():
			_cooldown2 = false
			for child in line_container.get_children():
				child.queue_free()
			for child in portrait_container_left.get_children():
				child.queue_free()
			for child in portrait_container_right.get_children():
				child.queue_free()
	)


func _append_choice(
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
		a: String,
		b: String,
		c: String,
) -> void:
	var choice: DialogueChoiceRow = choice_line_prefab.instantiate()
	choice.set_options.call_deferred(a, b, c)

	_append_component(choice, speaker, side)


func _append_line(text: String, speaker: DialogueSpeaker, side: DialogueLine2.Side) -> void:
	if _cooldown or _cooldown2:
		return
	_cooldown = true
	get_tree().create_timer(0.25).timeout.connect(func(): _cooldown = false)

	var dialogue_row: DialogueRow = line_prefab.instantiate()
	dialogue_row.set_text.call_deferred(text)
	var offset_x = 64.0 if side == DialogueLine2.Side.RIGHT else 0.0
	dialogue_row.set_deferred("offset_x", offset_x)

	_append_component(dialogue_row, speaker, side)

	# HACK: just wait a moment so that text is on screen when scroll starts
	await get_tree().create_timer(0.33).timeout
	if is_instance_valid(dialogue_row):
		dialogue_row.scroll_text.call_deferred()


func _append_component(
		dialogue_row: DialogueComponent,
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
) -> void:
	var portrait_left: DialoguePortrait = portrait_prefab.instantiate()
	var portrait_right: DialoguePortrait = portrait_prefab.instantiate()
	portrait_left.facing = Facing.Horizontal.RIGHT
	portrait_right.facing = Facing.Horizontal.LEFT

	portrait_left.speaker = speaker
	portrait_right.speaker = speaker

	var is_chain := _previous_speaker and _previous_speaker == speaker
	_previous_speaker = speaker

	if is_chain:
		portrait_right.modulate = Color.TRANSPARENT
		portrait_left.modulate = Color.TRANSPARENT
	elif side == DialogueLine2.Side.LEFT:
		portrait_right.modulate = Color.TRANSPARENT
	elif side == DialogueLine2.Side.RIGHT:
		portrait_left.modulate = Color.TRANSPARENT

	line_container.add_child(dialogue_row)
	portrait_container_left.add_child(portrait_left)
	portrait_container_right.add_child(portrait_right)
	dialogue_row.offset_changed.connect(line_container.queue_sort)
	portrait_left.offset_changed.connect(portrait_container_left.queue_sort)
	portrait_right.offset_changed.connect(portrait_container_right.queue_sort)

	portrait_left.speaker = speaker
	portrait_right.speaker = speaker

	var total_lines := line_container.get_child_count()
	for container in [line_container, portrait_container_left, portrait_container_right]:
		var delay := 0.0
		var child_idx := 0
		var delay_add: float = 0.1 if container == line_container else 1.25

		for child in container.get_children():
			var is_new_portrait: bool = child == portrait_left or child == portrait_right
			var offset := (64.0 + 16.0) if is_new_portrait else 64.0
			child.shift_up.call_deferred(delay, offset)
			delay += delay_add if child_idx >= total_lines - 4 else 0.0
			child_idx += 1
