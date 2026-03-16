@tool
class_name DialogueLineNew
extends Resource

# FIXME: implement as Resource
enum Speaker {
	PLAYER,
	CROW,
}
enum Side {
	LEFT,
	RIGHT,
}

@export var speaker: Speaker
@export var side: DialogueLineNew.Side = Side.LEFT
@export var text: String = "Just some placeholder text <3"
