@tool
class_name DialogueRow
extends MarginContainer

signal offset_changed

@export var anim_offset: float = 0.0:
	get:
		return anim_offset
	set(value):
		anim_offset = value
		offset_changed.emit()

@export_multiline("no_wrap")
var text: String = "Just some placeholder text <3":
	get:
		return Exports.delegate_get(_label, "text", "")
	set(value):
		Exports.delegate_set(_label, "text", value)

		if value and _label and _label.visible_characters != -1:
			_label.visible_characters = value.length()
@export_tool_button("Scroll text")
var debug_scroll_action = scroll_text
@export_tool_button("Shift up")
var debug_shift_up_action = shift_up
@export_tool_button("Reset")
var reset = _reset
var _shift_tween: Tween

@onready var _label: Label = $Text


func _ready() -> void:
	get_node("Text").visible_characters = 0


func scroll_text(line_text: String = "") -> void:
	if line_text.length() > 0:
		text = line_text

	var letter_count = _label.text.length()
	_label.visible_characters = 0

	while _label.visible_characters < letter_count:
		_label.visible_characters += 1
		await get_tree().create_timer(0.1).timeout


func shift_up(delay: float = 0.0) -> void:
	if _shift_tween:
		_reset()

	anim_offset = 64.0

	_shift_tween = create_tween()
	_shift_tween.set_ease(Tween.EASE_IN_OUT)
	_shift_tween.set_trans(Tween.TRANS_CUBIC)
	_shift_tween.set_parallel(false)
	_shift_tween.tween_interval(delay)
	_shift_tween.tween_property(self, "anim_offset", 64.0, 0.0)
	_shift_tween.set_parallel(true)
	_shift_tween.tween_property(self, "anim_offset", 0.0, 1.0)

	var child_idx: int = get_parent().get_children().find(self)
	var color_idx: int = min(4, get_parent().get_child_count() - (child_idx + 1))
	var alpha := 1.0 - (color_idx / 4.0)

	var color := self.modulate
	color.a = alpha
	_shift_tween.tween_property(self, "modulate", color, 0.75)

	await _shift_tween.finished


func _reset() -> void:
	if _shift_tween and _shift_tween.is_valid():
		_shift_tween.stop()
		_shift_tween.free()

	_shift_tween = null

	anim_offset = 0.0
