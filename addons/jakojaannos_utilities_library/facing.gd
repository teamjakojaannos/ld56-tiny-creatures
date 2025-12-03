@tool
class_name Facing

enum Direction {
	UP,
	RIGHT,
	DOWN,
	LEFT,
}
enum Horizontal {
	LEFT = Direction.LEFT,
	RIGHT = Direction.RIGHT,
}
enum Vertical {
	UP = Direction.UP,
	DOWN = Direction.DOWN,
}


static func opposite_h(facing: Horizontal) -> Horizontal:
	if facing == Horizontal.LEFT:
		return Horizontal.RIGHT
	else:
		return Horizontal.LEFT


static func as_vec_h(facing: Horizontal) -> Vector2:
	if facing == Horizontal.RIGHT:
		return Vector2.RIGHT
	else:
		return Vector2.LEFT


static func opposite_v(facing: Vertical) -> Vertical:
	if facing == Vertical.UP:
		return Vertical.DOWN
	else:
		return Vertical.UP


static func as_vec_v(facing: Vertical) -> Vector2:
	if facing == Vertical.UP:
		return Vector2.DOWN
	else:
		return Vector2.DOWN


static func opposite(facing: Direction) -> Direction:
	match facing:
		Direction.UP:
			return Direction.DOWN
		Direction.DOWN:
			return Direction.UP
		Direction.LEFT:
			return Direction.RIGHT
		Direction.RIGHT:
			return Direction.LEFT
		var not_valid:
			push_error(""""%s" is not a valid Direction!""" % not_valid)
			return Direction.UP


static func as_vec(facing: Direction) -> Vector2:
	match facing:
		Direction.UP:
			return Vector2.UP
		Direction.DOWN:
			return Vector2.DOWN
		Direction.LEFT:
			return Vector2.LEFT
		Direction.RIGHT:
			return Vector2.RIGHT
		var not_valid:
			push_error(""""%s" is not a valid Direction!""" % not_valid)
			return Vector2.UP


static func sign_h(facing: Horizontal) -> int:
	if facing == Horizontal.RIGHT:
		return 1
	else:
		return -1


static func sign_v(facing: Vertical) -> int:
	if facing == Vertical.DOWN:
		return 1
	else:
		return -1
