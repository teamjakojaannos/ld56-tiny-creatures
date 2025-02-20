extends StaticBody2D

@export var target: Node2D

func _physics_process(_delta: float) -> void:
	if target == null or target.is_queued_for_deletion():
		return

	global_position = target.global_position
