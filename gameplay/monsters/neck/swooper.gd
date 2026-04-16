@tool
class_name Swooper
extends PathFollow2D

var _tween: Tween


func swoop() -> void:
	reset(false)

	_tween = create_tween()
	_tween.set_trans(Tween.TRANS_CUBIC)
	_tween.set_ease(Tween.EASE_IN)
	_tween.tween_property(self, "progress_ratio", 1.0, 1.0)

	await _tween.finished


func unswoop() -> void:
	reset(true)

	_tween = create_tween()
	_tween.set_trans(Tween.TRANS_CUBIC)
	_tween.set_ease(Tween.EASE_IN_OUT)
	_tween.tween_property(self, "progress_ratio", 0.0, 3.0)

	await _tween.finished


func reset(is_swooped: bool) -> void:
	if _tween:
		_tween.stop()
		_tween = null

	if is_swooped:
		progress_ratio = 1.0
	else:
		progress_ratio = 0.0
