@tool
class_name DialogueRow
extends ControlAnimator

@export var side: DialogueEntry.PortraitSide:
	get:
		return Exports.delegate_get(_entry, "side", DialogueEntry.PortraitSide.LEFT)
	set(value):
		Exports.delegate_set(_entry, "side", value)

@export_multiline("no_wrap")
var text: String:
	get:
		return Exports.delegate_get(_entry, "text", "")
	set(value):
		Exports.delegate_set(_entry, "text", value)

@onready var _entry: DialogueEntry = $DialogueEntry
