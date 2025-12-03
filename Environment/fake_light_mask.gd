extends Node2D


func _process(_delta: float) -> void:
	var pc: PlayerController = Persistent.PlayerController
	var wisp := pc.wisp

	var is_player_past = wisp.global_position.y > global_position.y
	set_light_mask_layer(1, is_player_past)


func set_light_mask_layer(layer: int, state: bool) -> void:
	if state:
		light_mask |= (1 << layer)
	else:
		light_mask &= ~(1 << layer)
