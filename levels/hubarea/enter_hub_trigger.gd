extends Area2D

var _conv_01_safety_at_last: DialogueConversation = preload("uid://c7t1gwrllo38q")


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	Conversation.begin(_conv_01_safety_at_last)

	var pc: PlayerController = Persistent.PlayerController
	var wisp: Wisp = pc.wisp
	wisp.go_to($WispPosition.global_position, true)

	await Conversation.finished

	wisp.clear_go_to_target()
