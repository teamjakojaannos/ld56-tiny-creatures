extends RigidBody2D

@onready var player = get_node("../Player")

@export_category("Prewire")
@export var parts: Array[DampedSpringJoint2D] = []

func _integrate_forces(state: PhysicsDirectBodyState2D) -> void:
	var input_direction = player.InputDirection
	if input_direction:
		var arm_total_length = 0
		for part in parts:
			arm_total_length += part.length

		var target_position = input_direction * arm_total_length
		var distance = global_position.distance_to(target_position)

		if distance.dot(input_direction) > 0:
			distance = min(distance, arm_total_length)

		var distance_ratio = distance / arm_total_length
		var force = 3 * distance_ratio
		
		print("force: %s" % force)
		print("ratio: %s" % distance_ratio)
		state.linear_velocity += input_direction * force
