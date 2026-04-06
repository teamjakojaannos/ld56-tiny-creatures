@tool
extends Node

signal finished
signal option_chosen(option: int)

var is_in_conversation: bool

@onready var _ui: DialogueUINew = $UI


func _ready() -> void:
	# Destroy the tool instance UI, debug uses another instance
	if Engine.is_editor_hint():
		_ui.queue_free()
		return

	_ui.reset()


func show_lines(
		ui: DialogueUINew,
		lines: Array[DialogueLine2],
		is_continuation: bool = false,
):
	if not is_continuation:
		ui.reset()

	for line in lines:
		if line is DialogueTextLine2:
			var actual_lines = line.text.split("\n", false)
			for al in actual_lines:
				ui.show_line(al, line.speaker, line.side)

				await ui.input_progress
		elif line is DialogueChoiceLine2:
			var a: String = line.choice_text_a
			var b: String = line.choice_text_b
			var c: String = line.choice_text_c
			if line.number_of_choices == DialogueChoiceLine2.NumberOfChoices.TWO:
				c = ""

			await ui.show_choice(line.speaker, line.side, a, b, c)
			await ui.option_chosen
			var choice: int = ui.last_chosen_option
			option_chosen.emit(choice)

			var branch: DialogueConversation
			if choice == 0:
				branch = line.choice_branch_a
			elif choice == 1:
				branch = line.choice_branch_b
			if choice == 2:
				branch = line.choice_branch_c

			if branch != null:
				await show_lines(ui, branch.lines, true)
		else:
			printerr("Unsupported dialogue line type")

	await ui.end_dialogue()


func begin(conversation: DialogueConversation) -> void:
	if is_in_conversation:
		printerr("Tried to start another conversation while already in conversation!")
		return

	is_in_conversation = true
	await show_lines(_ui, conversation.lines)
	is_in_conversation = false
	finished.emit()
