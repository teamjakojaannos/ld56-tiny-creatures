@tool
@abstract
class_name NeckAiState
extends Node

enum Direction {
	NONE,
	FORWARD,
	BACKWARD,
}


func should_tick_detection() -> bool:
	return true


@abstract
func do_update(neck: Neck, delta: float) -> void


func detection_level_changed(neck: Neck) -> void:
	var detection_level := neck.detection_level
	if detection_level >= neck.attributes.attack_threshold:
		neck.ai = AttackState.new()
	elif detection_level >= neck.attributes.alert_threshold:
		neck.ai = AlertedState.new(neck.attributes.speed)


func random_idle_time(neck: Neck) -> float:
	var min_time := neck.attributes.min_idle_time
	var max_time := neck.attributes.max_idle_time
	return randf_range(min_time, max_time)


class AlertedState extends NeckAiState:
	const CLOSE_ENOUGH := 10.0

	var _speed: float
	var _time_passed: float = 0.0


	func _init(speed: float) -> void:
		_speed = speed


	func do_update(neck: Neck, delta: float) -> void:
		var relative_pos := _get_player_relative_x(neck)
		if is_nan(relative_pos):
			neck.detection_level = 0.0
			neck.ai = IdleState.new(random_idle_time(neck), Direction.NONE)
			return

		_time_passed += delta
		if _time_passed >= neck.attributes.alert_time:
			neck.detection_level = 0.0
			var direction := Direction.FORWARD if randf() < 0.5 else Direction.BACKWARD
			neck.ai = MovementState.new(direction, neck.attributes.speed)
			return

		_move_neck(neck, relative_pos, delta)


	func _get_player_relative_x(neck: Neck) -> float:
		var player := Persistent.player
		if not player:
			return NAN

		var player_pos := player.global_position
		var neck_pos := neck.global_position
		return neck_pos.x - player_pos.x


	func _move_neck(neck: Neck, relative_pos: float, delta: float) -> void:
		if absf(relative_pos) <= CLOSE_ENOUGH:
			return

		if not neck.path_follower:
			return

		var s := -signf(relative_pos)
		var movement = s * _speed * delta
		neck.path_follower.progress += movement


	func detection_level_changed(neck: Neck) -> void:
		var detection_level := neck.detection_level
		if detection_level >= neck.attributes.attack_threshold:
			neck.ai = AttackState.new()
			return

		if detection_level == 0.0:
			var direction := Direction.FORWARD if randf() < 0.5 else Direction.BACKWARD
			neck.ai = MovementState.new(direction, neck.attributes.speed)


class AttackState extends NeckAiState:
	var _animation_playing: bool = false


	func do_update(neck: Neck, _delta: float) -> void:
		if not _animation_playing:
			_animation_playing = true
			neck.play_attack_animation()


	func detection_level_changed(_neck: Neck) -> void:
		pass


	func should_tick_detection() -> bool:
		return false


class MovementState extends NeckAiState:
	var _time_passed: float
	var _direction: Direction
	var _speed: float


	func _init(direction: Direction, speed: float) -> void:
		_direction = direction
		_speed = speed


	func do_update(neck: Neck, delta: float) -> void:
		_time_passed += delta

		if _time_passed >= neck.attributes.move_time:
			_time_passed = 0.0

			var should_stop = randf() < neck.attributes.stop_move_chance
			if should_stop:
				var idle_time := random_idle_time(neck)
				neck.ai = IdleState.new(idle_time, Direction.NONE)

			var hide_chance := neck.attributes.go_underwater_chance
			var did_hide := neck.roll_to_go_under_water(hide_chance)
			if did_hide:
				return

		var did_reach_end := _move_neck(neck, delta)
		if did_reach_end:
			_end_of_line(neck)


	func _move_neck(neck: Neck, delta: float) -> bool:
		var path_follower := neck.path_follower
		if not path_follower:
			return true

		var s := 1.0 if _direction == Direction.FORWARD else -1.0
		var movement := s * _speed * delta
		path_follower.progress += movement

		var did_reach_end: bool
		if _direction == Direction.FORWARD:
			did_reach_end = path_follower.progress_ratio >= 1.0
		else:
			did_reach_end = path_follower.progress_ratio <= 0.0

		return did_reach_end


	func _end_of_line(neck: Neck) -> void:
		var opposite_direction: Direction
		if _direction == Direction.FORWARD:
			opposite_direction = Direction.BACKWARD
		else:
			opposite_direction = Direction.FORWARD

		var chance := neck.attributes.change_direction_instead_of_stopping_chance
		var should_keep_going := randf() < chance
		if should_keep_going:
			_direction = opposite_direction
			return

		chance = neck.attributes.go_underwater_chance
		var did_hide := neck.roll_to_go_under_water(chance)
		if did_hide:
			return

		var idle_time := random_idle_time(neck)
		neck.ai = IdleState.new(idle_time, opposite_direction)


class IdleState extends NeckAiState:
	var _time_waited: float = 0.0
	var _wait_time: float
	var _next_direction: Direction


	func _init(idle_time: float, next_direction: Direction):
		_wait_time = idle_time
		_next_direction = next_direction


	func do_update(neck: Neck, delta: float) -> void:
		_time_waited += delta
		if _time_waited < _wait_time:
			return

		var chance := neck.attributes.go_underwater_chance
		var did_hide := neck.roll_to_go_under_water(chance)
		if did_hide:
			return

		var next_direction: Direction
		if _next_direction == Direction.NONE:
			next_direction = Direction.FORWARD if randf() < 0.5 else Direction.BACKWARD
		else:
			next_direction = _next_direction

		neck.ai = MovementState.new(next_direction, neck.attributes.speed)


class UnderwaterState extends NeckAiState:
	var _underwater_time: float
	var _time_passed: float = 0.0
	var animation_done: bool = false


	func _init(underwater_time: float) -> void:
		_underwater_time = underwater_time


	func should_tick_detection() -> bool:
		return animation_done


	func detection_level_changed(_neck: Neck) -> void:
		pass


	func do_update(neck: Neck, delta: float) -> void:
		if not animation_done:
			return

		_time_passed += delta
		if _time_passed >= _underwater_time:
			_emerge_from_water(neck)


	func _emerge_from_water(neck: Neck) -> void:
		animation_done = false

		var chance := neck.attributes.emerge_at_player_chance
		var should_emerge_at_player := randf() < chance
		if should_emerge_at_player:
			neck.emerge_from_water_near_player()
			return

		chance = neck.attributes.emerge_at_same_location_chance
		var should_emerge_at_same_spot := randf() < chance
		var position: float
		if should_emerge_at_same_spot and neck.path_follower:
			position = neck.path_follower.progress_ratio
		else:
			position = randf()

		neck.emerge_from_water_at_position(position)


class WaitForTriggerState extends NeckAiState:
	var _is_animation_set: bool


	func do_update(neck: Neck, _delta: float) -> void:
		if not _is_animation_set:
			_is_animation_set = true
			neck.force_underwater()


	func should_tick_detection() -> bool:
		return false
