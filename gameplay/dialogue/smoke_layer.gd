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
		_refresh.call_deferred()

var DEFAULT: NoiseTexture3D = preload("uid://cguqcmrr27038")

@onready var _texture_rect: TextureRect


func _refresh() -> void:
	var mat := _texture_rect.material as ShaderMaterial
	mat.set_shader_parameter("noise_texture", noise_texture)
