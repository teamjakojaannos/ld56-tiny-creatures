extends Node

signal finished

@onready var _ui: DialogueUINew = $UI


func _ready() -> void:
	_ui.reset()


func begin(_conversation: DialogueConversation) -> void:
	_ui.reset()

	for line in _conversation.lines:
		if line is DialogueTextLine2:
			var actual_lines = line.text.split("\n", false)
			for al in actual_lines:
				_ui.show_line(al, line.speaker, line.side)

				await _ui.input_progress
		else:
			printerr("Unsupported dialogue line type")

	await _ui.end_dialogue()
	finished.emit()
