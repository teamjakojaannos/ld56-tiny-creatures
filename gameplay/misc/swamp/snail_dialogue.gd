@tool
extends PlayerTriggerArea2D

var _conversation: DialogueConversation = preload("uid://ceco6qt4lw75w")


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered() -> void:
	Conversation.begin(_conversation)
