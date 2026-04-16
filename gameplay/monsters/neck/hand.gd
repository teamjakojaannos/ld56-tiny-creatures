@tool
class_name NeckHand
extends Node2D

@export_tool_button("Grab")
var debug_grab = grab
@export_tool_button("Appear")
var debug_appear = appear
@export_tool_button("Disappear")
var debug_disappear = disappear
var _tween: Tween

@onready var sprite_origin: Node2D = $SpriteOrigin
@onready var sprite: AnimatedSprite2D = $SpriteOrigin/Sprite
@onready var sprite_top: AnimatedSprite2D = $SpriteOrigin/SpriteTop
@onready var player_pivot: Node2D = $SpriteOrigin/GrabbedPlayerPosition


func _ready() -> void:
	if not Engine.is_editor_hint():
		$SpriteOrigin/GrabbedPlayerPosition/PreviewPlayer.queue_free()


func appear() -> void:
	reset(false, false)

	_tween = create_tween()
	_tween.set_trans(Tween.TRANS_CUBIC)
	_tween.set_ease(Tween.EASE_OUT)

	_tween.set_parallel(true)
	_tween.tween_property(sprite_origin, "position", Vector2.ZERO, 0.66)
	_tween.tween_property(sprite_origin, "modulate", Color.WHITE, 0.5)

	await _tween.finished


func disappear() -> void:
	reset(true, true)

	_tween = create_tween()
	_tween.set_trans(Tween.TRANS_CUBIC)
	_tween.set_ease(Tween.EASE_OUT)

	_tween.set_parallel(true)
	_tween.tween_property(sprite_origin, "position", Vector2.DOWN * 16.0, 2.5)
	_tween.tween_property(sprite_origin, "modulate", Color.TRANSPARENT, 1.0)

	await _tween.finished


func grab() -> void:
	reset(false, true)
	sprite.play("grab")
	sprite_top.play("grab")

	await sprite.animation_finished


func reset(closed: bool, is_vis: bool) -> void:
	sprite.stop()
	sprite_top.stop()
	if _tween:
		_tween.stop()
		_tween = null

	if closed:
		sprite.animation = "grab"
		sprite.frame = sprite.sprite_frames.get_frame_count("grab") - 1
		sprite_top.animation = "grab"
		sprite_top.frame = sprite.sprite_frames.get_frame_count("grab") - 1
	else:
		sprite.animation = "open"
		sprite.frame = sprite.sprite_frames.get_frame_count("open") - 1
		sprite_top.animation = "open"
		sprite_top.frame = sprite.sprite_frames.get_frame_count("open") - 1

	if is_vis:
		sprite_origin.modulate = Color.WHITE
		sprite_origin.position = Vector2.ZERO
	else:
		sprite_origin.modulate = Color.TRANSPARENT
		sprite_origin.position = Vector2.DOWN * 16.0
