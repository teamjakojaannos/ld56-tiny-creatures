@tool
class_name Swooper
extends PathFollow2D

var _tween: Tween


func swoop() -> void:
	reset(false)

	_tween = create_tween()
	_tween.set_trans(Tween.TRANS_CUBIC)
	_tween.set_ease(Tween.EASE_OUT)
	_tween.tween_property(self, "progress_ratio", 1.0, 0.5)

	await _tween.finished


func reset(is_swooped: bool) -> void:
	if _tween:
		_tween.stop()
		_tween = null

	if is_swooped:
		progress_ratio = 1.0
	else:
		progress_ratio = 0.0
