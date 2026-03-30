@tool
class_name DialogueRow
extends DialogueComponent

@export_multiline("no_wrap")
var text: String = "Just some placeholder text <3":
	get:
		return Exports.delegate_get(_label, "text", "")
	set(value):
		Exports.delegate_set(_label, "text", value)

		if value and _label and _label.visible_characters != -1:
			_label.visible_characters = value.length()
var _is_skipped: bool = false

@onready var _label: Label = $Text


func _ready() -> void:
	$Text.visible_characters = 0


func set_text(line_text: String) -> void:
	text = line_text
	$Text.visible_characters = 0


func scroll_text() -> void:
	var letter_count = _label.text.length()

	# HACK: Safeguard against button-smashing through dialogue.
	# The shift up animation adds a short delay between the scroll starting, and
	# if a call to _reset occurs during that window (e.g. button-smashing)
	# visible_characters is first set to string length in _reset and almost
	# immediately after to zero here. Bail out if that is about to happen.
	if _is_skipped:
		return

	_label.visible_characters = 0

	while _label.visible_characters < letter_count:
		_label.visible_characters += 1
		await get_tree().create_timer(0.05).timeout


func _reset() -> void:
	super._reset()

	var letter_count = _label.text.length()
	_label.visible_characters = letter_count
	_is_skipped = true
