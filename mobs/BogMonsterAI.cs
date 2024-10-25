using Godot;

namespace BogMonsterStuff;

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
		if (detectionLevel >= monster.stats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel >= monster.stats.alertThreshold) {
			monster.ai = new AlertedState(monster.stats.speed);
			return;
		}
	}

	internal static float randomIdleTime(BogMonster monster) {
		var (min, max) = monster.stats.idleTime;
		return monster.rng.RandfRange(min, max);
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
		if (timePassed >= monster.stats.moveTime) {
			timePassed = 0.0f;

			var stopMovement = monster.rng.DiceRoll(monster.stats.stopMoveChance);
			if (stopMovement) {
				var idleTime = randomIdleTime(monster);
				monster.ai = new IdleState(idleTime, nextDirection: null);
				return;
			}

			var wentUnderwater = monster.rollToGoUnderwater(monster.stats.goUnderwaterChance);
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
		var keepGoing = monster.rng.Randf() < monster.stats.changeDirectionInsteadOfStoppingChance;
		if (keepGoing) {
			direction = direction.opposite();
			return;
		}

		var wentUnderwater = monster.rollToGoUnderwater(monster.stats.goUnderwaterChance);
		if (wentUnderwater) {
			return;
		}

		var idleTime = randomIdleTime(monster);
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
			var wentUnderwater = monster.rollToGoUnderwater(monster.stats.goUnderwaterChance);
			if (wentUnderwater) {
				return;
			}

			bool goingForward;

			if (nextDirection is Direction direction) {
				goingForward = direction.isForward();
			} else {
				goingForward = RandomNumberGeneratorExtension.RandomBool(monster.rng);
			}

			monster.ai = new MovementState(goingForward, monster.stats.speed);
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



		var emergeAtPlayer = monster.rng.DiceRoll(monster.stats.emergeAtPlayerChance);
		if (emergeAtPlayer) {
			monster.emergeFromWaterNearPlayer();
		} else {
			var emergeAtSameSpot = monster.rng.DiceRoll(monster.stats.emergeAtSameLocationChance);
			float position;
			if (emergeAtSameSpot) {
				position = monster.ProgressRatio;
			} else {
				position = monster.rng.Randf();
			}

			monster.emergeFromWaterAtPosition(position);
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
			monster.ai = new IdleState(randomIdleTime(monster), null);
			return;
		}

		timePassed += delta;
		if (timePassed >= monster.stats.alertTime) {
			monster.detectionLevel = 0.0f;
			monster.ai = new MovementState(RandomNumberGeneratorExtension.RandomBool(monster.rng), monster.stats.speed);
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
		if (detectionLevel >= monster.stats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel == 0.0f) {
			monster.ai = new MovementState(RandomNumberGeneratorExtension.RandomBool(monster.rng), monster.stats.speed);
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

public class WaitUntilTriggerIsTriggeredState : BogMonsterAIState {

	public bool animationSet;

	public override void doUpdate(BogMonster monster, float delta) {
		if (!animationSet) {
			animationSet = true;
			monster.playGoUnderwaterAnimationThisIsVeryHackyThingDontUse();
		}
	}

	public override bool shouldTickDetection() {
		return false;
	}
}
