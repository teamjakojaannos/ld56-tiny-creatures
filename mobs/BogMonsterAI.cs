using Godot;

using Jakojaannos.WisperingWoods.Util;

namespace BogMonsterStuff;

public enum Direction {
	Forward,
	Backward,
}

public static class DirectionExtension {
	public static bool IsForward(this Direction direction) => direction == Direction.Forward;

	public static Direction Opposite(this Direction direction) {
		return direction == Direction.Forward ? Direction.Backward : Direction.Forward;
	}
}

public abstract class BogMonsterAIState {
	public abstract void DoUpdate(BogMonster monster, float delta);

	public virtual bool ShouldTickDetection() {
		return true;
	}

	public virtual void DetectionLevelChanged(BogMonster monster) {
		float detectionLevel = monster.detectionLevel;
		if (detectionLevel >= monster.Stats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel >= monster.Stats.alertThreshold) {
			monster.ai = new AlertedState(monster.Stats.speed);
			return;
		}
	}

	internal static float RandomIdleTime(BogMonster monster) {
		var (min, max) = monster.Stats.IdleTime;
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

	public override void DoUpdate(BogMonster monster, float delta) {
		timePassed += delta;
		if (timePassed >= monster.Stats.moveTime) {
			timePassed = 0.0f;

			var stopMovement = monster.rng.DiceRoll(monster.Stats.stopMoveChance);
			if (stopMovement) {
				var idleTime = RandomIdleTime(monster);
				monster.ai = new IdleState(idleTime, nextDirection: null);
				return;
			}

			var wentUnderwater = monster.RollToGoUnderwater(monster.Stats.goUnderwaterChance);
			if (wentUnderwater) {
				return;
			}
		}

		var reachedEnd = MoveMonster(monster, delta);

		if (reachedEnd) {
			EndOfLine(monster);
		}
	}

	private bool MoveMonster(BogMonster monster, float delta) {
		var sign = direction.IsForward() ? 1.0f : -1.0f;
		var movement = sign * speed * delta;

		monster.Progress += movement;

		var reachedEnd = direction.IsForward()
			? monster.ProgressRatio >= 1.0f
			: monster.ProgressRatio <= 0.0f;

		return reachedEnd;
	}

	private void EndOfLine(BogMonster monster) {
		var keepGoing = monster.rng.Randf() < monster.Stats.changeDirectionInsteadOfStoppingChance;
		if (keepGoing) {
			direction = direction.Opposite();
			return;
		}

		var wentUnderwater = monster.RollToGoUnderwater(monster.Stats.goUnderwaterChance);
		if (wentUnderwater) {
			return;
		}

		var idleTime = RandomIdleTime(monster);
		monster.ai = new IdleState(idleTime, nextDirection: direction.Opposite());
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

	public override void DoUpdate(BogMonster monster, float delta) {
		timeWaited += delta;
		if (timeWaited >= waitTime) {
			var wentUnderwater = monster.RollToGoUnderwater(monster.Stats.goUnderwaterChance);
			if (wentUnderwater) {
				return;
			}

			bool goingForward;

			if (nextDirection is Direction direction) {
				goingForward = direction.IsForward();
			} else {
				goingForward = RandomNumberGeneratorExtension.RandomBool(monster.rng);
			}

			monster.ai = new MovementState(goingForward, monster.Stats.speed);
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

	public override void DoUpdate(BogMonster monster, float delta) {
		if (!animationDone) {
			return;
		}

		timePassed += delta;
		if (timePassed >= underwaterTime) {
			EmergeFromWater(monster);
		}
	}

	public override bool ShouldTickDetection() {
		// animation playing -> don't increase/decrease detection level
		return animationDone;
	}

	private void EmergeFromWater(BogMonster monster) {
		animationDone = false;

		var emergeAtPlayer = monster.rng.DiceRoll(monster.Stats.emergeAtPlayerChance);
		if (emergeAtPlayer) {
			monster.EmergeFromWaterNearPlayer();
		} else {
			var emergeAtSameSpot = monster.rng.DiceRoll(monster.Stats.emergeAtSameLocationChance);
			float position;
			if (emergeAtSameSpot) {
				position = monster.ProgressRatio;
			} else {
				position = monster.rng.Randf();
			}

			monster.EmergeFromWaterAtPosition(position);
		}
	}

	public override void DetectionLevelChanged(BogMonster monster) { }
}

public class AlertedState : BogMonsterAIState {
	private const float closeEnough = 10.0f;

	private float speed;
	private float timePassed;

	public AlertedState(float speed) {
		this.speed = speed;
	}

	public override void DoUpdate(BogMonster monster, float delta) {
		var r = GetPlayerXPositionRelativeToMonster(monster);
		if (r is not float relative) {
			// can't find player for some reason
			monster.detectionLevel = 0.0f;
			monster.ai = new IdleState(RandomIdleTime(monster), null);
			return;
		}

		timePassed += delta;
		if (timePassed >= monster.Stats.alertTime) {
			monster.detectionLevel = 0.0f;
			monster.ai = new MovementState(monster.rng.RandomBool(), monster.Stats.speed);
			return;
		}

		MoveMonster(monster, relative, delta);
	}

	public static float? GetPlayerXPositionRelativeToMonster(BogMonster monster) {
		var playerRef = monster.GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return null;
		}

		var playerPosition = player.GlobalPosition;
		var monsterPosition = monster.GlobalPosition;

		return monsterPosition.X - playerPosition.X;
	}

	private void MoveMonster(BogMonster monster, float relativeX, float delta) {
		if (Mathf.Abs(relativeX) <= closeEnough) {
			return;
		}

		var sign = -Mathf.Sign(relativeX);
		var movement = sign * speed * delta;

		monster.Progress += movement;
	}

	public override void DetectionLevelChanged(BogMonster monster) {
		float detectionLevel = monster.detectionLevel;
		if (detectionLevel >= monster.Stats.attackThreshold) {
			monster.ai = new AttackState();
			return;
		}

		if (detectionLevel == 0.0f) {
			monster.ai = new MovementState(monster.rng.RandomBool(), monster.Stats.speed);
			return;
		}
	}
}

public class AttackState : BogMonsterAIState {
	public bool animationPlaying;

	public override void DoUpdate(BogMonster monster, float delta) {
		if (!animationPlaying) {
			animationPlaying = true;
			monster.PlayAttackAnimation();
		}
	}

	public override void DetectionLevelChanged(BogMonster monster) { }

	public override bool ShouldTickDetection() {
		return false;
	}
}

public class WaitUntilTriggerIsTriggeredState : BogMonsterAIState {

	public bool animationSet;

	public override void DoUpdate(BogMonster monster, float delta) {
		if (!animationSet) {
			animationSet = true;
			monster.PlayGoUnderwaterAnimationThisIsVeryHackyThingDontUse();
		}
	}

	public override bool ShouldTickDetection() {
		return false;
	}
}
