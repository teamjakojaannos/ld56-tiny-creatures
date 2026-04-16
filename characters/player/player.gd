@tool
class_name Player
extends CharacterBody2D

signal moving_start
signal moving_stop
signal dead
signal teleported

@export var move_speed: float = 150.0
@export var friction: float = 10.0
@export var controller: MovementController

var facing: Facing.Direction = Facing.Direction.RIGHT
var is_moving: bool = false
var is_prone: bool = false
var is_slowed: bool = false
var is_invulnerable: bool = false
var is_dead: bool = false
var is_in_trouble: bool = false
var move_speed_modifier: float:
	get:
		return 0.5 if is_slowed else 1.0
var can_move: bool:
	get:
		return not is_dead \
		&& not Conversation.is_in_conversation

@onready var rig: PlayerSprite = $Sprite


func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint():
		return

	var input_dir := Vector2.ZERO
	if controller and can_move:
		input_dir = controller.input_direction.limit_length(1.0)

	_process_movement(input_dir, delta)

	if input_dir:
		if input_dir.x < 0:
			facing = Facing.Direction.LEFT
		elif input_dir.x > 0:
			facing = Facing.Direction.RIGHT
		elif input_dir.y < 0:
			facing = Facing.Direction.UP
		else:
			facing = Facing.Direction.DOWN


## Teleport player to a given node. Player is reparented to be a sibling of the
## target node.
##
## Used for e.g. teleporting player back to hub after death.
func teleport(to: Node2D) -> void:
	if not to.is_inside_tree() or to.is_queued_for_deletion():
		assert(false, "Teleport target \"%s\" isn't a valid node in tree." % to)
		return

	var new_parent = to.get_parent()
	if not new_parent:
		new_parent = to

	if get_parent():
		reparent(new_parent, false)
	else:
		new_parent.add_child(self)

	global_position = to.global_position
	reset_physics_interpolation()

	teleported.emit()


func die(force: bool = false) -> void:
	# Cannot die if already dead.
	if is_dead:
		return

	# Cannot die if invulnerable, unless someone *really* wants to kill you.
	if is_invulnerable and not force:
		return

	is_dead = true
	dead.emit()


func _process_movement(input_dir: Vector2, delta: float) -> void:
	if input_dir:
		velocity = input_dir * move_speed * move_speed_modifier
	else:
		var d := velocity.length() * friction * delta
		velocity = velocity.move_toward(Vector2.ZERO, d)

	var is_stopped = input_dir.length_squared() <= 0.001
	if not is_stopped and not is_moving:
		is_moving = true
		moving_start.emit()
	elif is_stopped and is_moving:
		is_moving = false
		moving_stop.emit()

	move_and_slide()
