using Godot;

namespace BogMonsterStuff;

public static class BogMonsterStats {

	// moves for this many seconds, then rolls to change state
	public const float moveTime = 3.0f;

	// chance to stop movement and change state
	public const float stopMoveChance = 0.40f;

	// chance to stop movement and change state
	public const float changeDirectionInsteadOfStoppingChance = 0.10f;

	public const float goUnderwaterChance = 0.40f;

	// min/max time to stay underwater
	public static (float, float) underwaterTime = (1.0f, 3.0f);

	// min and max idle time
	public static (float, float) idleTime = (0.5f, 2.0f);

	public const float emergeAtPlayerChance = 0.35f;

	public const float alertTime = 7.5f;
	public const float alertThreshold = 40.0f;
	public const float attackThreshold = 100.0f;
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

	public virtual bool shouldTickDetection() {
		return true;
	}

	public virtual void detectionLevelChanged(BogMonster monster) {
		float detectionLevel = monster.detectionLevel;
		if (detectionLevel >= BogMonsterStats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel >= BogMonsterStats.alertThreshold) {
			monster.ai = new AlertedState(monster.speed);
			return;
		}
	}

	internal static float randomIdleTime(RandomNumberGenerator rng) {
		var (min, max) = BogMonsterStats.idleTime;
		return rng.RandfRange(min, max);
	}
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

			var wentUnderwater = monster.rollToGoUnderwater(BogMonsterStats.goUnderwaterChance);
			if (wentUnderwater) {
				return;
			}
		}

		var reachedEnd = moveMonster(monster, delta);

		if (reachedEnd) {
			endOfLine(monster);
		}
	}

	private bool moveMonster(BogMonster monster, float delta) {
		var sign = direction.isForward() ? 1.0f : -1.0f;
		var movement = sign * speed * delta;

		monster.Progress += movement;

		var reachedEnd = direction.isForward()
			? monster.ProgressRatio >= 1.0f
			: monster.ProgressRatio <= 0.0f;

		return reachedEnd;
	}


	private void endOfLine(BogMonster monster) {
		var keepGoing = monster.rng.Randf() < BogMonsterStats.changeDirectionInsteadOfStoppingChance;
		if (keepGoing) {
			direction = direction.opposite();
			return;
		}

		var wentUnderwater = monster.rollToGoUnderwater(BogMonsterStats.goUnderwaterChance);
		if (wentUnderwater) {
			return;
		}

		var idleTime = randomIdleTime(monster.rng);
		monster.ai = new IdleState(idleTime, nextDirection: direction.opposite());
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
			var wentUnderwater = monster.rollToGoUnderwater(BogMonsterStats.goUnderwaterChance);
			if (wentUnderwater) {
				return;
			}

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

public class UnderwaterState : BogMonsterAIState {
	private float timePassed;
	private float underwaterTime;
	public bool animationDone;

	public UnderwaterState(float underwaterTime) {
		this.underwaterTime = underwaterTime;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		if (!animationDone) {
			return;
		}

		timePassed += delta;
		if (timePassed >= underwaterTime) {
			emergeFromWater(monster);
		}
	}

	public override bool shouldTickDetection() {
		// animation playing -> don't increase/decrease detection level
		return animationDone;
	}

	private void emergeFromWater(BogMonster monster) {
		animationDone = false;
		var emergeAtPlayer = monster.rng.Randf() < BogMonsterStats.emergeAtPlayerChance;
		if (emergeAtPlayer) {
			monster.emergeFromWaterNearPlayer();
		} else {
			var randomPosition = monster.rng.Randf();
			monster.emergeFromWaterAtPosition(randomPosition);
		}
	}

	public override void detectionLevelChanged(BogMonster monster) { }
}

public class AlertedState : BogMonsterAIState {
	private const float closeEnough = 10.0f;

	private float speed;
	private float timePassed;

	public AlertedState(float speed) {
		this.speed = speed;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		var r = getPlayerXPositionRelativeToMonster(monster);
		if (r is not float relative) {
			// can't find player for some reason
			monster.detectionLevel = 0.0f;
			monster.ai = new IdleState(randomIdleTime(monster.rng), null);
			return;
		}

		timePassed += delta;
		if (timePassed >= BogMonsterStats.alertTime) {
			monster.detectionLevel = 0.0f;
			monster.ai = new MovementState(Util.randomBool(monster.rng), monster.speed);
			return;
		}

		moveMonster(monster, relative, delta);
	}

	public float? getPlayerXPositionRelativeToMonster(BogMonster monster) {
		var playerRef = monster.GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return null;
		}

		var playerPosition = player.GlobalPosition;
		var monsterPosition = monster.GlobalPosition;

		return monsterPosition.X - playerPosition.X;
	}

	private void moveMonster(BogMonster monster, float relativeX, float delta) {
		if (Mathf.Abs(relativeX) <= closeEnough) {
			return;
		}

		var sign = -Mathf.Sign(relativeX);
		var movement = sign * speed * delta;

		monster.Progress += movement;
	}

	public override void detectionLevelChanged(BogMonster monster) {
		float detectionLevel = monster.detectionLevel;
		if (detectionLevel >= BogMonsterStats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel == 0.0f) {
			monster.ai = new MovementState(Util.randomBool(monster.rng), monster.speed);
			return;
		}
	}
}

public class AttackState : BogMonsterAIState {
	public bool animationPlaying;

	public override void doUpdate(BogMonster monster, float delta) {
		if (!animationPlaying) {
			animationPlaying = true;
			monster.playAttackAnimation();
		}
	}

	public override void detectionLevelChanged(BogMonster monster) { }

	public override bool shouldTickDetection() {
		return false;
	}
}
