@tool
class_name Chaser
extends CharacterBody2D

@export var move_speed: float = 60.0

var move_target: Vector2 = Vector2.INF
var _bodies_in_cone: Array[Node2D]
var _is_nav_setup_done: bool = false
var _is_attacking: bool = false
var _is_idling: bool = false
var _can_move: bool:
	get:
		return not _is_attacking

@onready var sight_cone: Area2D = $SightCone
@onready var sight_ray: RayCast2D = $LineOfSight
@onready var nav_agent: NavigationAgent2D = $NavigationAgent
@onready var sprite: AnimatedSprite2D = $Sprite


func _ready() -> void:
	Signals.try_connect(sight_cone.body_entered, _sight_cone_entered)
	Signals.try_connect(sight_cone.body_exited, _sight_cone_exited)

	_wait_for_nav_setup.call_deferred()


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return

	var monster: CharacterBody2D = self
	var player := Persistent.player

	if not _is_attacking:
		var facing = "front" if velocity.y >= 0 else "back"

		if facing == "front":
			sprite.flip_h = velocity.x >= 0
		else:
			sprite.flip_h = velocity.x <= 0

		if velocity.is_zero_approx():
			sprite.play("idle_%s" % facing)
		else:
			sprite.play("walk_%s" % facing)

	if not _can_move:
		return

	if _try_chase(monster, player, delta):
		if nav_agent.is_navigation_finished() and not _is_attacking:
			_try_attack(monster, player)
		return
	if _try_seek(monster, delta):
		return

	_roam(monster, delta)


func _wait_for_nav_setup() -> void:
	await get_tree().physics_frame
	_is_nav_setup_done = true


func _try_attack(monster: CharacterBody2D, player: Player) -> void:
	_is_attacking = true

	if player.is_dead:
		sprite.play("idle_front")
		return

	sprite.play("attack")
	await sprite.animation_finished

	var player_pos := player.global_position
	var monster_pos := monster.global_position
	var distance_to_player := player_pos.distance_to(monster_pos)

	if distance_to_player < 30.0:
		player.is_prone = true
		player.die()

	_is_attacking = false


func _sight_cone_entered(body: Node2D) -> void:
	_bodies_in_cone.push_back(body)


func _sight_cone_exited(body: Node2D) -> void:
	_bodies_in_cone.erase(body)


func _is_in_cone(node: Node2D) -> bool:
	return _bodies_in_cone.has(node)


func _is_in_los(node: Node2D) -> bool:
	var target_pos := node.global_position
	var relative_pos := target_pos - sight_ray.global_position

	sight_ray.target_position = relative_pos
	sight_ray.force_raycast_update()

	if not sight_ray.is_colliding():
		return false

	var collider := sight_ray.get_collider()
	return collider == node


func _try_chase(monster: CharacterBody2D, player: Player, delta: float) -> bool:
	var is_player_in_sight_cone: bool = _is_in_cone(player)
	var is_player_in_line_of_sight: bool = _is_in_los(player)
	if not is_player_in_sight_cone or not is_player_in_line_of_sight:
		return false

	move_target = player.global_position
	if not _try_face_target(monster, delta):
		return false

	return _try_move()


func _try_face_target(monster: Node2D, delta: float) -> bool:
	var angle_offset := deg_to_rad(90.0)
	var turn_speed := deg_to_rad(45.0)
	var pos := monster.global_position

	var goal_angle := pos.angle_to_point(move_target) + angle_offset
	var current_angle := sight_cone.rotation

	var d := turn_speed * delta
	var new_angle := rotate_toward(current_angle, goal_angle, d)
	sight_cone.rotation = new_angle

	var remaining := absf(angle_difference(goal_angle, new_angle))
	return remaining < 0.1


func _try_move() -> bool:
	var monster: CharacterBody2D = self

	if not _is_nav_setup_done:
		return false

	if not move_target.is_finite():
		return false

	nav_agent.target_position = move_target
	if nav_agent.is_navigation_finished():
		move_target = Vector2.INF
		return true

	var current_pos := monster.global_position
	var next_pos := nav_agent.get_next_path_position()
	var direction := current_pos.direction_to(next_pos)

	var new_velocity := direction * move_speed
	monster.velocity = new_velocity
	monster.move_and_slide()

	return true


func _try_seek(monster: CharacterBody2D, delta: float) -> bool:
	if not _try_face_target(monster, delta):
		return false

	return _try_move()


func _roam(monster: CharacterBody2D, delta: float) -> bool:
	if _is_idling:
		return false

	if not move_target.is_finite():
		_is_idling = true
		get_tree().create_timer(2.0).timeout.connect(
			func():
				_is_idling = false
		)

		var markers := get_tree().get_nodes_in_group("chaser_roam_marker")
		move_target = markers.pick_random().global_position

		if not _try_face_target(monster, delta):
			return false

	return _try_move()
