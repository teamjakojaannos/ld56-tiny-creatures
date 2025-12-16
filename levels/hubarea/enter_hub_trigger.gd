extends Area2D


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	DialogueMan.ActiveDialogue = $Dialogue
	DialogueMan.StartDialogue()

	var pc: PlayerController = Persistent.PlayerController
	var wisp: Wisp = pc.wisp
	wisp.go_to($WispPosition.global_position, true)

	await DialogueMan.DialogueFinished
	DialogueMan.ActiveDialogue = null

	wisp.clear_go_to_target()
