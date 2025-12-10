extends Area2D


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	DialogueMan.ActiveDialogue = $Dialogue
	DialogueMan.StartDialogue()

	var wisp: Wisp = Persistent.PlayerController.wisp
	wisp.go_to($WispPosition.global_position)

	await DialogueMan.DialogueFinished
	wisp.Release()
