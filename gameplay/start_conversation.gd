@tool
class_name StartConversation
extends Node

@export var conversation: DialogueConversation
@export var touch_trigger: TouchTrigger


func _ready() -> void:
	touch_trigger = Nodes.find_if_null(get_parent(), touch_trigger, TouchTrigger)
	if touch_trigger:
		Signals.try_connect(touch_trigger.fire, start)


func start(_cause: Node2D) -> void:
	Conversation.begin(conversation)
