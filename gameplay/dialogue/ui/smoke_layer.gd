@tool
extends ColorRect

@export var noise_texture: NoiseTexture3D = DEFAULT:
	get:
		return noise_texture
	set(value):
		if value == null:
			printerr("Tried to set noise texture to null")
			return

		noise_texture = value
		_refresh_texture.call_deferred()
@export var scroll_time_scale_h: float = 0.0:
	get:
		return scroll_time_scale_h
	set(value):
		scroll_time_scale_h = value
		_refresh_time_scale.call_deferred()
@export var scroll_time_scale_v: float = 0.1:
	get:
		return scroll_time_scale_v
	set(value):
		scroll_time_scale_v = value
		_refresh_time_scale.call_deferred()
@export var scroll_time_scale_z: float = 1.0:
	get:
		return scroll_time_scale_z
	set(value):
		scroll_time_scale_z = value
		_refresh_time_scale.call_deferred()

var DEFAULT: NoiseTexture3D = preload("uid://cguqcmrr27038")

@onready var _texture_rect: TextureRect = $TextureRect


func _refresh_texture() -> void:
	_texture_rect.set_instance_shader_parameter("noise_texture", noise_texture)


func _refresh_time_scale() -> void:
	_texture_rect.set_instance_shader_parameter("scroll_time_scale_h", scroll_time_scale_h)
	_texture_rect.set_instance_shader_parameter("scroll_time_scale_v", scroll_time_scale_v)
	_texture_rect.set_instance_shader_parameter("scroll_time_scale_z", scroll_time_scale_z)
