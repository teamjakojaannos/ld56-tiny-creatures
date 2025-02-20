extends RigidBody2D

func _integrate_forces(state: PhysicsDirectBodyState2D) -> void:
	var direction = Input.get_vector("left", "right", "up", "down")
	if direction:
		# FIXME: scale by distance from target position
		state.linear_velocity += direction * 20.0
