@tool
@abstract
class_name DialogueLine2
extends Resource

enum Side {
	LEFT,
	RIGHT,
}

@export var side: DialogueLine2.Side = DialogueLine2.Side.LEFT
@export var speaker: DialogueSpeaker
