public static class BogMonsterStats {
	public static (float, float) waitTime = (0.5f, 2.0f);
}

public enum Direction {
	Forward,
	Backward,
}

public static class DirectionExtension {
	public static bool isForward(this Direction direction) => direction == Direction.Forward;

	public static Direction opposite(this Direction direction) {
		return direction == Direction.Forward ? Direction.Backward : Direction.Forward;
	}
}

public abstract class BogMonsterAIState {
	public abstract void doUpdate(BogMonster monster, float delta);
}

public class MovementState : BogMonsterAIState {

	private Direction direction;

	private float speed;

	public MovementState(bool goingForward, float speed) {
		this.direction = goingForward ? Direction.Forward : Direction.Backward;
		this.speed = speed;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		var sign = direction.isForward() ? 1.0f : -1.0f;
		var movement = sign * speed * delta;

		monster.Progress += movement;

		var reachedEnd = direction.isForward()
			? monster.ProgressRatio >= 1.0f
			: monster.ProgressRatio <= 0.0f;

		if (reachedEnd) {
			endOfLine(monster);
		}
	}

	private void endOfLine(BogMonster monster) {
		var (min, max) = BogMonsterStats.waitTime;
		var waitTime = monster.rng.RandfRange(min, max);
		monster.ai = new WaitState(waitTime, nextDirection: direction.opposite());
	}
}

public class WaitState : BogMonsterAIState {
	private float timeWaited;
	private float waitTime;

	private Direction? nextDirection;

	public WaitState(float waitTime, Direction? nextDirection) {
		this.waitTime = waitTime;
		this.nextDirection = nextDirection;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		timeWaited += delta;
		if (timeWaited >= waitTime) {
			bool goingForward;

			if (nextDirection is Direction direction) {
				goingForward = direction.isForward();
			} else {
				goingForward = Util.randomBool(monster.rng);
			}

			monster.ai = new MovementState(goingForward, monster.speed);
		}
	}
}


