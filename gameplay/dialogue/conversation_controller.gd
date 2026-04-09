@tool
class_name ConversationController
extends Node

enum UIInput {
	PROGRESS,
	PREV_CHOICE,
	NEXT_CHOICE,
	CHOOSE_A,
	CHOOSE_B,
	CHOOSE_C,
}

@export var debug_conversation: DialogueConversation

@export_group("Debug Controls")
@export_tool_button("Begin")
var debug_begin = _debug_begin
@export_tool_button("Reset")
var debug_reset = _debug_reset
@export_tool_button("Next line / Choose highlighted option")
var debug_progress = handle_input.bind(UIInput.PROGRESS)
@export_tool_button("Highlight Previous")
var debug_previous_option = handle_input.bind(UIInput.PREV_CHOICE)
@export_tool_button("Highlight Next")
var debug_next_option = handle_input.bind(UIInput.NEXT_CHOICE)
@export_tool_button("Highlight A")
var debug_option_a = handle_input.bind(UIInput.CHOOSE_A)
@export_tool_button("Highlight B")
var debug_option_b = handle_input.bind(UIInput.CHOOSE_B)
@export_tool_button("Highlight C")
var debug_option_c = handle_input.bind(UIInput.CHOOSE_C)

@onready var _c: ConversationManager = $".."


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("gui_up"):
		handle_input(UIInput.PREV_CHOICE)
	elif event.is_action_pressed("gui_down"):
		handle_input(UIInput.NEXT_CHOICE)
	elif event.is_action_pressed("gui_accept"):
		handle_input(UIInput.PROGRESS)
	elif event.is_action_pressed("dialogue_option_1"):
		handle_input(UIInput.CHOOSE_A)
	elif event.is_action_pressed("dialogue_option_2"):
		handle_input(UIInput.CHOOSE_B)
	elif event.is_action_pressed("dialogue_option_3"):
		handle_input(UIInput.CHOOSE_C)


func handle_input(input: UIInput) -> void:
	if not _c.is_in_conversation or _c.is_in_transition:
		return

	if _c.is_waiting_for_choice:
		_handle_choice_input(input)
	else:
		_handle_normal_input(input)


func _debug_reset() -> void:
	_c.reset()


func _debug_begin() -> void:
	_c.begin(debug_conversation)


func _handle_normal_input(input: UIInput) -> void:
	match input:
		UIInput.PROGRESS:
			_c.progress()
		_:
			pass


func _handle_choice_input(input: UIInput) -> void:
	match input:
		UIInput.PROGRESS:
			_c.choose_option()
		UIInput.PREV_CHOICE:
			_c.current_option = max(0, _c.current_option - 1)
		UIInput.NEXT_CHOICE:
			_c.current_option = min(_c.max_option, _c.current_option + 1)
		UIInput.CHOOSE_A:
			_c.current_option = 0
		UIInput.CHOOSE_B:
			_c.current_option = 1
		UIInput.CHOOSE_C:
			_c.current_option = 2
