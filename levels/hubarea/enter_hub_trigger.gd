extends Area2D


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	DialogueMan.ActiveDialogue = $Dialogue
	DialogueMan.StartDialogue()

	var pc: PlayerController = Persistent.PlayerController
	var wisp: Wisp = pc.wisp
	var player: Player = pc.player
	wisp.go_to($WispPosition.global_position, true)
	player.MovementEnabled = false

	await DialogueMan.DialogueFinished
	wisp.clear_go_to_target()
	player.MovementEnabled = true
