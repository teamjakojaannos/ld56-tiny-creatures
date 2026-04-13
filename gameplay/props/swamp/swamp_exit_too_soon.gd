@tool
extends PlayerTriggerArea2D

var _conversation: DialogueConversation = preload("uid://cp418hflorxlq")


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered() -> void:
	var _is_every_swamp_wisp_saved: bool = Persistent.wisps_saved >= 7
	if _is_every_swamp_wisp_saved:
		return

	Conversation.begin(_conversation)
