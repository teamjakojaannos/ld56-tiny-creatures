extends Area2D


func _on_touch_trigger_fire(_cause: Node2D) -> void:
	DialogueMan.ActiveDialogue = $Dialogue
	DialogueMan.StartDialogue()
