using Godot;

namespace BogMonsterStuff;

public static class BogMonsterStats {

	// moves for this many seconds, then rolls to change state
	public const float moveTime = 1.5f;

	// chance to stop movement and change state
	public const float stopMoveChance = 0.25f;

	// min and max wait time
	public static (float, float) idleTime = (0.5f, 2.0f);
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

	private float timePassed;

	private Direction direction;

	private float speed;

	public MovementState(bool goingForward, float speed) {
		this.direction = goingForward ? Direction.Forward : Direction.Backward;
		this.speed = speed;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		timePassed += delta;
		if (timePassed >= BogMonsterStats.moveTime) {
			timePassed = 0.0f;
			var stopMovement = monster.rng.Randf() < BogMonsterStats.stopMoveChance;
			if (stopMovement) {
				var idleTime = randomIdleTime(monster.rng);
				monster.ai = new IdleState(idleTime, nextDirection: null);
				return;
			}
		}

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
		var idleTime = randomIdleTime(monster.rng);
		monster.ai = new IdleState(idleTime, nextDirection: direction.opposite());
	}

	private static float randomIdleTime(RandomNumberGenerator rng) {
		var (min, max) = BogMonsterStats.idleTime;
		return rng.RandfRange(min, max);
	}
}

public class IdleState : BogMonsterAIState {
	private float timeWaited;
	private float waitTime;

	private Direction? nextDirection;

	public IdleState(float waitTime, Direction? nextDirection) {
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


