@tool
extends PlayerTriggerArea2D

var _conversation: DialogueConversation = preload("uid://d3e5rt6g6iaew")
var _is_first_time: bool = true


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered() -> void:
	if not _is_first_time:
		return

	_is_first_time = true
	Conversation.begin(_conversation)
