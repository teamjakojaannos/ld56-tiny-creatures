@tool
class_name WispPosition
extends Marker2D

@export var trigger: PlayerTriggerArea2D


func _ready() -> void:
	trigger = Nodes.find_if_null(get_parent(), trigger, PlayerTriggerArea2D)

	if trigger:
		Signals.try_connect(trigger.player_entered, _on_player_entered)
		Signals.try_connect(trigger.player_exited, _on_player_exited)


func _on_player_entered(_player: Player, wisp: Wisp) -> void:
	wisp.go_to(global_position, true)


func _on_player_exited(_player: Player, wisp: Wisp) -> void:
	wisp.clear_go_to_target()
