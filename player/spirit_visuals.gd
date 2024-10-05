extends Node2D

@onready var target: Node2D = get_parent().get_node("Spirit") as Node2D

func _process(delta: float) -> void:
	global_position = target.global_position
