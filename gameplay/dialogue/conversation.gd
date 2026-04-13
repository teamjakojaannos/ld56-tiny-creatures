@tool
class_name ConversationManager
extends Node

signal finished
signal next_line
signal option_chosen(option: int)

var current_option: int = 0:
	get:
		return current_option
	set(value):
		if value > max_option or value < 0:
			return
		current_option = value

		if is_instance_valid(_ui):
			_ui.highlight_option(value)
var current_line: DialogueLine2:
	get:
		if not is_in_conversation:
			return null
		return _active_conversation[_line_idx]
var max_option: int = 2
var is_waiting_for_choice: bool:
	get:
		return current_line is DialogueChoiceLine2
var is_in_conversation: bool:
	get:
		return _active_conversation != null and _active_conversation.size() > 0
var is_in_transition: bool:
	get:
		return is_instance_valid(_ui) && _ui.is_in_transition
var _active_conversation: Array[DialogueLine2] = []
var _line_idx: int = 0

@onready var _ui: DialogueUINew = $UI


func _ready() -> void:
	# If in editor, destroy the autoload UI instance on load. The UI covers
	# entire screen, consuming any mouse input events, preventing editor usage.
	if Engine.is_editor_hint() and get_viewport() is Window:
		get_parent().remove_child(self)
		return

	reset()


func reset() -> void:
	current_option = 0
	max_option = 2

	_active_conversation = []
	_line_idx = 0

	if is_instance_valid(_ui):
		_ui.reset()


func progress() -> void:
	_line_idx += 1
	if _line_idx >= _active_conversation.size():
		end_conversation()
		return

	var line := _active_conversation[_line_idx]
	if line is DialogueTextLine2:
		_ui.show_line(line.text, line.speaker, line.side)
	elif line is DialogueChoiceLine2:
		var a: String = line.choice_text_a
		var b: String = line.choice_text_b
		var c: String = line.choice_text_c
		if line.number_of_choices == DialogueChoiceLine2.NumberOfChoices.TWO:
			c = ""
			max_option = 1
		else:
			max_option = 2

		current_option = 0
		_ui.show_choice(line.speaker, line.side, a, b, c)

	next_line.emit()


func choose_option() -> void:
	var active_choice: DialogueChoiceLine2
	if current_line is not DialogueChoiceLine2:
		printerr("Tried to choose option when current line is non-interactive!")
		return

	active_choice = current_line

	var branch: DialogueConversation
	if current_option == 0:
		branch = active_choice.choice_branch_a
	elif current_option == 1:
		branch = active_choice.choice_branch_b
	if current_option == 2:
		branch = active_choice.choice_branch_c

	if branch and not branch.lines.is_empty():
		_append_conversation(branch)

	option_chosen.emit(current_option)

	progress()


func end_conversation() -> void:
	await _ui.end_dialogue()
	reset()

	finished.emit()


func begin(conversation: DialogueConversation) -> void:
	if is_in_conversation:
		printerr("Tried to start another conversation while already in conversation!")
		return

	if conversation.lines.is_empty():
		printerr("Tried to start an empty conversation!")
		return

	_append_conversation(conversation)
	is_in_conversation = true

	_line_idx = -1
	progress()


func _append_conversation(conversation: DialogueConversation) -> void:
	var all_lines: Array[DialogueLine2] = []
	for original in conversation.lines:
		if original is DialogueTextLine2:
			var split := _split_line(original)
			all_lines.append_array(split)
		elif original is DialogueChoiceLine2:
			all_lines.push_back(original)
		else:
			printerr("Unsupported dialogue line type")

	_active_conversation.append_array(all_lines)


func _split_line(line: DialogueTextLine2) -> Array[DialogueLine2]:
	var result: Array[DialogueLine2] = []

	var split_text_lines := line.text.split("\n", false)
	for split_text in split_text_lines:
		var split_instance := DialogueTextLine2.new()
		split_instance.text = split_text
		split_instance.side = line.side
		split_instance.speaker = line.speaker

		result.push_back(split_instance)

	return result
