@tool
class_name WispPOI
extends PlayerTriggerArea2D

signal activate
signal deactivate

## The target position the wisp moves to when the trigger is entered.
@export var wisp_target: Node2D

var _was_activated: bool = false
var _is_player_inside: bool = false


func _ready() -> void:
	super._ready()

	Signals.try_connect(player_entered, _on_player_entered)
	Signals.try_connect(player_exited, _on_player_exited)


func _on_player_entered(_player: Player, wisp: Wisp) -> void:
	_is_player_inside = true

	await wisp.go_to(wisp_target.global_position, true)

	if _is_player_inside:
		_was_activated = true
		activate.emit()


func _on_player_exited(_player: Player, wisp: Wisp) -> void:
	_is_player_inside = false
	wisp.clear_go_to_target()

	if _was_activated:
		deactivate.emit()
	_was_activated = false
