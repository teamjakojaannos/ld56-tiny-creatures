@tool
class_name Wisp
extends RigidBody2D

signal target_reached

@export var target_radius: float = 5.0
@export var max_velocity: float = 250.0
@export var follow_target: WispFollowTarget
@export var player: PlayerCharacter

var target_global_position: Vector2:
	get:
		if _go_to_target.is_finite():
			return _go_to_target
		return follow_target.global_position
var sprite_visible: bool:
	get:
		return sprite.visible
	set(value):
		sprite.visible = value
var _go_to_target: Vector2 = Vector2.INF
var _target_locked: bool = false

@onready var sprite: Node2D = $WispVisuals/Sprite
@onready var remark: RemarkBubble = $UnshadedLayer/RemarkBubble


func _ready() -> void:
	remark.visible = false


func _physics_process(_delta: float) -> void:
	if Engine.is_editor_hint():
		return

	_apply_movement_forces()
	_check_go_to_target_status()


func go_to(target: Vector2, stay: bool = false) -> void:
	_go_to_target = target
	await target_reached

	if not stay:
		clear_go_to_target()


func clear_go_to_target() -> void:
	if _target_locked:
		return

	_go_to_target = Vector2.INF


func lock_target() -> void:
	_target_locked = true


func unlock_target() -> void:
	_target_locked = true


func say(text: String, duration: float = 2.0) -> void:
	await remark.show_remark(text, duration)


func _check_go_to_target_status() -> void:
	if not _go_to_target.is_finite():
		return

	var dist_to_target := global_position.distance_to(_go_to_target)
	if dist_to_target <= target_radius:
		target_reached.emit()


func _apply_movement_forces() -> void:
	var is_player_moving = true # TODO
	linear_damp = 1.5 if is_player_moving else 2.0
	if _go_to_target.is_finite():
		linear_damp = 3.5

	var target_pos := target_global_position
	var dist_sq := global_position.distance_squared_to(target_pos)
	var dist_ratio := dist_sq / follow_target.follow_distance

	var force := 10.0 * dist_ratio
	var direction := global_position.direction_to(target_pos)

	apply_central_force(direction * force)
	follow_target.debug_radius = sqrt(force)


func _integrate_forces(state: PhysicsDirectBodyState2D) -> void:
	state.linear_velocity = state.linear_velocity.limit_length(max_velocity)

	# HACK: If very close to a target, just lerp there
	var dist_to_target := global_position.distance_to(_go_to_target)
	if _go_to_target.is_finite():
		if dist_to_target < 50.0:
			global_position = global_position.lerp(_go_to_target, 0.05)
			state.linear_velocity *= 0.5
		elif dist_to_target < 100.0:
			state.linear_velocity *= 0.95
