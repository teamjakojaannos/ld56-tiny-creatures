@tool
class_name ScreenFader
extends CanvasLayer

@export_range(0.0, 1.0) var fade_progress: float = 0.0:
	get:
		return fade_progress
	set(value):
		fade_progress = clamp(value, 0.0, 1.0)

		if _overlay != null:
			_overlay.visible = fade_progress > 0.0
			_overlay.color.a = fade_progress

var _overlay: ColorRect


func _ready() -> void:
	_overlay = ColorRect.new()
	_overlay.color = Color.BLACK
	_overlay.set_anchors_preset(Control.PRESET_FULL_RECT, false)

	add_child(_overlay, false, Node.INTERNAL_MODE_FRONT)
