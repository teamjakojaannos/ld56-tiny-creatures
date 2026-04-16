@tool
extends PlayerTriggerArea2D


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)


func _on_player_entered(_player: Player, _wisp: Wisp) -> void:
	queue_free()
