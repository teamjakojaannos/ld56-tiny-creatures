@tool
extends HBoxContainer

@onready var portrait = $PortraitFrame/Character
@onready var text_content = $TextContent

@export var speaker_is_on_left = true:
	set(new_value):
		speaker_is_on_left = new_value
		refresh()

@export var text = "":
	set(new_value):
		text = new_value
		if text_content != null:
			text_content.text = new_value

func refresh() -> void:
	if speaker_is_on_left:
		layout_direction = LAYOUT_DIRECTION_LTR
		if portrait != null:
			portrait.flip_h = true
	else:
		layout_direction = LAYOUT_DIRECTION_RTL
		if portrait != null:
			portrait.flip_h = false

func _ready() -> void:
	refresh()
