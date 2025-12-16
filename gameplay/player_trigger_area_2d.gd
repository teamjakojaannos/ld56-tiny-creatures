@tool
class_name PlayerTriggerArea2D
extends Area2D

signal player_entered(player: Player, wisp: Wisp)
signal player_exited(player: Player, wisp: Wisp)


func _ready() -> void:
	Signals.try_connect(body_entered, _on_body_entered)
	Signals.try_connect(body_exited, _on_body_exited)


func _on_body_entered(node: Node2D) -> void:
	if node is not Player or Engine.is_editor_hint():
		return

	var wisp: Wisp = Persistent.PlayerController.wisp
	player_entered.emit(node, wisp)


func _on_body_exited(node: Node2D) -> void:
	if node is not Player or Engine.is_editor_hint():
		return

	var wisp: Wisp = Persistent.PlayerController.wisp
	player_exited.emit(node, wisp)
