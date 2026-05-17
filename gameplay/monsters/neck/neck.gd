@tool
class_name Neck
extends Node2D

@export var attributes: NeckAttributes
@export var path_follower: PathFollow2D
@export var default_state_is_wait: bool = false

var detection_level: float = 0.0
var can_go_underwater: bool:
	get:
		return _underwater_cooldown.is_stopped()
var ai: NeckAiState
var _water_splash: PackedScene = preload("uid://dx5xfep76dve6")
var _target: Player
var _is_attack_in_progress: bool = false

@onready var _hand: NeckHand = $Hand
@onready var _underwater_cooldown: Timer = $UnderwaterCooldown
@onready var _animations: AnimationPlayer = $Animations
@onready var _los: RayCast2D = $LineOfSight
@onready var _attack_sfx: AudioStreamPlayer2D = $Attack


func _ready() -> void:
	if not Engine.is_editor_hint():
		$Sprite.play("idle")
		$Hand.reset(false, false)
		ai = _default_state()

	Signals.try_connect(Persistent.player_respawned, _player_respawned)
	Signals.try_connect($Animations.animation_finished, _animation_finished)
	Signals.try_connect($SightCone.body_entered, _sight_cone_entered)
	Signals.try_connect($SightCone.body_exited, _sight_cone_exited)


func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint():
		return

	ai.do_update(self, delta)

	if ai.should_tick_detection():
		_update_detection(delta)

	var player = Persistent.player
	if _is_attack_in_progress and not player.is_dead:
		_sync_hand_location(player, 1.0 * delta)


func roll_to_go_under_water(chance: float) -> bool:
	if not can_go_underwater:
		return false

	var should_hide := randf() < chance
	if should_hide:
		go_underwater()
		return true

	return false


func go_underwater(time_mult: float = 1.0) -> void:
	var min_time := attributes.min_underwater_time
	var max_time := attributes.max_underwater_time
	var underwater_time := randf_range(min_time, max_time)
	ai = NeckAiState.UnderwaterState.new(underwater_time * time_mult)
	_animations.play("go_underwater")
	_underwater_cooldown.start()


func force_underwater() -> void:
	_animations.play("go_underwater", -1, 1.0, true)
	_animations.advance(0.0)


func emerge_from_water_at_position(pos: float) -> void:
	if path_follower:
		path_follower.progress_ratio = pos

	_animations.play("emerge_from_water", -1, attributes.emerge_animation_speed)


func emerge_from_water_near_player() -> void:
	var pos_ratio := _get_follower_progress_ratio_near_player()
	emerge_from_water_at_position(pos_ratio)


func do_attack() -> void:
	var player := Persistent.player
	if not player or player.is_dead or player.is_in_trouble:
		return

	if _is_attack_in_progress:
		return

	_is_attack_in_progress = true

	# Position hand
	var flip_h := player.global_position.x < global_position.x
	_hand.scale.x = -1 if flip_h else 1
	_sync_hand_location(player, 1.0)

	# Spawn water splash
	var splash = _water_splash.instantiate()
	get_parent().add_child(splash)
	splash.global_position = player.global_position

	# Start the attack sequence
	player.is_slowed = true
	await _hand.appear()
	_attack_sfx.play()

	# Wait a moment to give player time to react
	await get_tree().create_timer(attributes.attack_time).timeout

	# KILL KILL KILL
	await _hand.grab()
	player.is_slowed = false
	_try_kill(player)

	# Delay so player has time to process what happened
	await get_tree().create_timer(1.5).timeout

	# Finish the attack. If player is grabbed, they are reparented to the hand
	# and will disappear alongside the hand.
	go_underwater(0.25)
	await _hand.disappear()
	_is_attack_in_progress = false


func _update_detection(delta) -> void:
	var old_level := detection_level

	var can_see_player := _raycast_to_player()
	if can_see_player:
		detection_level += attributes.detection_gain * delta
	else:
		detection_level -= attributes.detection_decay * delta

	detection_level = clamp(detection_level, 0.0, 100.0)

	if detection_level != old_level:
		ai.detection_level_changed(self)


func _raycast_to_player() -> bool:
	if not _target or not _los:
		return false

	var player_pos: = _target.global_position
	var target_pos := player_pos - _los.global_position
	_los.target_position = target_pos

	_los.force_raycast_update()
	if not _los.is_colliding():
		return false

	var collider := _los.get_collider()
	var can_see_player := collider is Player

	return can_see_player


func _default_state() -> NeckAiState:
	if default_state_is_wait:
		return NeckAiState.WaitForTriggerState.new()

	return NeckAiState.MovementState.new(NeckAiState.Direction.FORWARD, attributes.speed)


func _player_respawned() -> void:
	_animations.play("emerge_from_water")
	ai = _default_state()


func _sight_cone_entered(body: Node2D) -> void:
	if body is Player:
		_target = body


func _sight_cone_exited(body: Node2D) -> void:
	if body is Player:
		_target = null


func _animation_finished(animation: String) -> void:
	match animation:
		"emerge_from_water":
			_emerge_from_water_done()
		"go_underwater":
			_go_underwater_animation_done()
		_:
			pass


func _emerge_from_water_done() -> void:
	var d = NeckAiState.Direction.FORWARD if randf() < 0.5 else NeckAiState.Direction.BACKWARD
	if path_follower:
		ai = NeckAiState.MovementState.new(d, attributes.speed)
	else:
		var min_time := attributes.min_idle_time
		var max_time := attributes.max_idle_time
		var time := randf_range(min_time, max_time)
		ai = NeckAiState.IdleState.new(time, d)


func _go_underwater_animation_done() -> void:
	var player := Persistent.player

	# Player is dead, stay underwater
	if player and player.is_dead:
		return

	if ai is NeckAiState.UnderwaterState:
		ai.animation_done = true


func _sync_hand_location(player: Player, delta: float) -> void:
	if not _hand:
		return

	var pos := _hand.global_position
	var distance := pos.distance_to(player.global_position)
	_hand.global_position = pos.move_toward(player.global_position, distance * delta)


func _try_kill(player: Player) -> void:
	if not _hand.is_player_in_danger:
		return

	player.reparent(_hand.player_pivot, false)
	player.position = Vector2.ZERO

	player.is_in_trouble = true
	player.die()


func _get_follower_progress_ratio_near_player() -> float:
	if not path_follower:
		return randf()

	var parent = path_follower.get_parent()
	if parent is not Path2D:
		return randf()
	var path: Path2D = parent

	var player := Persistent.player
	if not player:
		return randf()

	var player_pos := player.global_position

	var points = path.curve.get_baked_points()
	var first: Vector2 = points[0] + path.global_position
	var last: Vector2 = points[points.size() - 1] + path.global_position

	var min_x := minf(first.x, last.x)
	var max_x := maxf(first.x, last.x)
	var length := absf(max_x - min_x)
	if length == 0.0:
		return randf()

	var progress := (player_pos.x - min_x) / length
	return clampf(progress, 0.0, 1.0)
