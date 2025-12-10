@tool
extends Area2D

@export var target: Node2D


func _ready() -> void:
	Signals.try_connect(area_entered, _on_area_entered)
	Signals.try_connect(area_exited, _on_area_exited)


func _on_area_entered(node: Node2D) -> void:
	if node is not PlayerCharacter or Engine.is_editor_hint():
		return

	var wisp: WispCharacter = node.Wisp
	_player_entered_area(node, wisp)


func _on_area_exited(node: Node2D) -> void:
	if node is not PlayerCharacter or Engine.is_editor_hint():
		return

	var wisp: WispCharacter = node.Wisp
	_player_exited_area(node, wisp)


func _player_entered_area(_player: PlayerCharacter, _wisp: WispCharacter) -> void:
	pass


func _player_exited_area(_player: PlayerCharacter, _wisp: WispCharacter) -> void:
	pass
