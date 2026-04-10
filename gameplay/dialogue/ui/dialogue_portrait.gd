@tool
class_name DialoguePortrait
extends DialogueComponent

var speaker: DialogueSpeaker:
	get:
		return speaker
	set(value):
		speaker = value
		_refresh.call_deferred()
var facing: Facing.Horizontal = Facing.Horizontal.LEFT

@onready var portrait_texture: TextureRect = $Texture


func _refresh() -> void:
	portrait_texture.texture = speaker.texture
	portrait_texture.flip_h = speaker.portrait_facing != facing
