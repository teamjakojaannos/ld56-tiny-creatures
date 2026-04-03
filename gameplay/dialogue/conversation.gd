extends Node

signal finished

@onready var _ui: DialogueUINew = $UI


static func show_lines(ui: DialogueUINew, lines: Array[DialogueLine2]):
	ui.reset()

	for line in lines:
		if line is DialogueTextLine2:
			var actual_lines = line.text.split("\n", false)
			for al in actual_lines:
				ui.show_line(al, line.speaker, line.side)

				await ui.input_progress
		else:
			printerr("Unsupported dialogue line type")

	await ui.end_dialogue()


func _ready() -> void:
	_ui.reset()


func begin(conversation: DialogueConversation) -> void:
	show_lines(_ui, conversation.lines)
	finished.emit()
