extends Area2D


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	DialogueMan.ActiveDialogue = $Dialogue
	DialogueMan.StartDialogue()

	var wisp: WispCharacter = Persistent.PlayerController.wisp
	wisp.GoToSync($WispPosition.global_position)

	await DialogueMan.DialogueFinished
	wisp.Release()
