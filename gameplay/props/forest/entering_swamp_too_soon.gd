@tool
extends PlayerTriggerArea2D

var _conversation: DialogueConversation = preload("uid://dwwin08c5olh1")


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered() -> void:
	var _is_every_forest_wisp_saved: bool = Persistent.wisps_saved >= 5
	if _is_every_forest_wisp_saved:
		return

	Conversation.begin(_conversation)
