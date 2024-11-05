extends Node2D

func _process(_delta: float) -> void:
	var player = Persistent.Player
	var lantern = player.get_node("SpiritTarget/SpiritVisuals") as Node2D
	var is_player_past = lantern.global_position.y > global_position.y
	set_light_mask_layer(1, is_player_past)

func set_light_mask_layer(layer: int, state: bool) -> void:
	if state:
		light_mask |= (1 << layer)
	else:
		light_mask &= ~(1 << layer)
