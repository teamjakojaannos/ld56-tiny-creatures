@tool
extends CanvasLayer

@onready var FadeColor = get_node("FadeColor")

@export_range(0.0, 1.0) var FadeProgress: float = 0.0:
	get:
		return FadeProgress
	set(value):
		FadeProgress = clamp(value, 0.0, 1.0)

		if FadeColor != null:
			FadeColor.visible = FadeProgress > 0.0
			FadeColor.color.a = FadeProgress
		
