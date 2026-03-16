@tool
class_name DialoguePortrait
extends MarginContainer

signal offset_changed

@export var anim_offset: float = 0.0:
	get:
		return anim_offset
	set(value):
		anim_offset = value
		offset_changed.emit()

var _shift_tween: Tween


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

	if modulate != Color.TRANSPARENT:
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

	_shift_tween = null

	anim_offset = 0.0
