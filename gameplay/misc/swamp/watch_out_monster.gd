@tool
extends PlayerTriggerArea2D

@export var neck: Neck

var _conversation: DialogueConversation = preload("uid://tplfxqc34njy")
var _is_first_time: bool = true


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered(_player: Player, _wisp: Wisp) -> void:
	if not _is_first_time:
		return

	_is_first_time = true
	Conversation.begin(_conversation)

	# HACK: the conversation has three lines, show monster on the last one
	await Conversation.next_line
	await Conversation.next_line

	neck.emerge_from_water_at_position(0.0)
