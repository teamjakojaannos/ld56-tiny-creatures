@tool
class_name CameraManager
extends Node2D

signal fade_finished

@export var zoom: float = 1.0:
	get:
		return zoom
	set(value):
		zoom = value
		_refresh_zoom.call_deferred()

@onready var camera: Camera2D = $Shaker/Camera
@onready var _shaker: Shaker = $Shaker
@onready var _fader: ScreenFader = $ScreenFader
@onready var _thud_sfx: AudioStreamPlayer = $ThudSfx


func detach_from_player() -> void:
	reparent(Persistent, true)
	reset_physics_interpolation()


func attach_to_player() -> void:
	reparent(Persistent.player)
	global_position = Persistent.player.global_position
	reset_physics_interpolation()


func shake(magnitude: float, duration: float, direction: Vector2 = Vector2.ZERO) -> void:
	_shaker.magnitude = magnitude
	_shaker.shake_direction = direction.limit_length(1.0)
	_shaker.strength = 1.0

	var tween = create_tween()
	tween.tween_property(_shaker, "strength", 0.0, duration)
	tween.set_ease(Tween.EASE_IN)
	tween.set_trans(Tween.TRANS_EXPO)

	_thud_sfx.play()


func set_fully_visible() -> void:
	_fader.fade_progress = 0.0


func set_fully_obscured() -> void:
	_fader.fade_progress = 1.0


func fade_to_black(duration: float = 2.5) -> void:
	await _tween_fade_progress(duration, 0.0, 1.0)


func fade_to_visible(duration: float = 2.5) -> void:
	await _tween_fade_progress(duration, 1.0, 0.0)


func _refresh_zoom() -> void:
	camera.zoom = Vector2(zoom, zoom)


func _tween_fade_progress(duration: float, from: float, to: float) -> void:
	_fader.fade_progress = from

	var tween = create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(_fader, "fade_progress", to, duration)

	await tween.finished
	fade_finished.emit()
