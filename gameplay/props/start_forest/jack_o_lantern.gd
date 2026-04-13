extends Node2D

var _dialogue_shown: bool = false

@onready var anim: AnimationPlayer = $AnimationPlayer


func _on_activate() -> void:
	anim.play("default")
	anim.advance(0)
	anim.seek(0.8, true)

	var wisp: Wisp = Persistent.wisp
	wisp.sprite_visible = false

	if not _dialogue_shown:
		_dialogue_shown = true
		_show_first_time_dialogue()


func _show_first_time_dialogue() -> void:
	var wisp: Wisp = Persistent.wisp
	var player: RemarkBubble = Persistent.player.get_node("UnshadedLayer/RemarkBubble")

	await wisp.say("BOO!", 3.0)
	await player.show_remark("So, no longer in a hurry?", 2.5)
	await wisp.say("Shut up.", 1.5)
	await wisp.say("You are no fun.", 2.0)


func _on_deactivate() -> void:
	anim.play("RESET")
	anim.advance(0)
	Persistent.wisp.sprite_visible = true
