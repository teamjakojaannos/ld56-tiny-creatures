class_name DebugController
extends Node

@export var player: Player


func _input(event: InputEvent) -> void:
	if not player:
		return

	if event.is_action_pressed("dbg_kill"):
		player.die(true)
