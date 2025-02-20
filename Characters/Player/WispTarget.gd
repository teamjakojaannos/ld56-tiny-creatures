extends RigidBody2D

@onready var player = get_node("../Player")
@onready var debug_marker: StaticBody2D = get_node("../DbgArmTargetPos")
@onready var debug_marker_shape: CollisionShape2D = get_node("../DbgArmTargetPos/Shape")

var arm_total_length = 75
var rest_damp = 2
var moving_damp = 1.5

func _physics_process(delta: float) -> void:
	var input_direction = player.InputDirection
	if input_direction:
		linear_damp = moving_damp
		debug_marker.global_position = player.global_position + input_direction * arm_total_length
	else:
		linear_damp = rest_damp

	var target_position = debug_marker.global_position
	var distance = global_position.distance_to(target_position)
	var direction = global_position.direction_to(target_position)

	var distance_ratio = (distance * distance) / arm_total_length
	var force = 10.0 * distance_ratio

	var marker_circle = debug_marker_shape.shape as CircleShape2D
	marker_circle.radius = sqrt(force)

	apply_central_force(direction * force)
