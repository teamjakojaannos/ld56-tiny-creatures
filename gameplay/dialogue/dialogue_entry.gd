@tool
class_name DialogueEntry
extends HBoxContainer

enum PortraitSide {
	LEFT,
	RIGHT,
}

@export var side: PortraitSide = PortraitSide.LEFT:
	get:
		return side
	set(value):
		side = value
		_refresh.call_deferred()

@export_multiline("no_wrap")
var text: String = "Just some placeholder text <3":
	get:
		return Exports.delegate_get(_label, "text", "")
	set(value):
		Exports.delegate_set(_label, "text", value)

		if value and _label and _label.visible_characters != -1:
			_label.visible_characters = value.length()
var portrait_rect: TextureRect:
	get:
		return _portrait_left if side == PortraitSide.LEFT else _portrait_right
var portrait: Texture2D:
	get:
		return _portrait_texture
	set(value):
		_portrait_texture = value
		_refresh.call_deferred()
@export_tool_button("Scroll")
var debug_scroll_action = _scroll
var _portrait_texture: Texture2D = preload("uid://d1rmimef34ha7")

@onready var _portrait_left: TextureRect = $PortraitLeft
@onready var _portrait_right: TextureRect = $PortraitRight
@onready var _label: Label = $Text


func _scroll() -> void:
	var letter_count = _label.text.length()
	_label.visible_characters = 0

	while _label.visible_characters < letter_count:
		_label.visible_characters += 1
		await get_tree().create_timer(0.1).timeout


func _refresh() -> void:
	if _portrait_left:
		_portrait_left.texture = _portrait_texture
	if _portrait_right:
		_portrait_right.texture = _portrait_texture

	_portrait_left.modulate.a = 0.0
	_portrait_right.modulate.a = 0.0

	portrait_rect.modulate.a = 1.0
