@tool
extends PlayerTriggerArea2D

var _conversation: DialogueConversation = preload("uid://1wo4whdiovt6")


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered() -> void:
	Conversation.begin(_conversation)
