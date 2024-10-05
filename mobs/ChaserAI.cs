using Godot;

namespace ChaserStuff;

public static class ChaserStats {
	// idle for this many seconds, then change state
	public const float idleTime = 5.0f;

	// wander for this many seconds, then change state
	public const float wanderTime = 15.0f;

	// seek for player this many seconds, then change state
	public const float seekTime = 15.0f;

	// time spent looking around before turning head
	public const float lookTime = 3.0f;

	// motivation to name things properly ends here
	public static (float, float) howFarNewTargetShouldBe = (100.0f, 1000.0f);

	// in seek state: one in N chance to pick new target we it will walk, otherwise look around
	public const int pickNewWalkTargetWeight = 3;
}


public abstract class ChaserAI {
	public abstract void doUpdate(Chaser chaser, float delta);
}

public class IdleState : ChaserAI {

	private float timePassed;

	public override void doUpdate(Chaser chaser, float delta) {
		timePassed += delta;

		if (timePassed >= ChaserStats.idleTime) {
			chaser.startWandering();
		}
	}
}

public class WanderState : ChaserAI {

	private const float closeEnough = 10.0f;

	private Vector2 target;
	private float timePassed;

	public WanderState(Vector2 target) {
		this.target = target;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		timePassed += delta;
		if (timePassed >= ChaserStats.wanderTime) {
			chaser.startIdling();
			return;
		}

		var hasReachedTarget = target.DistanceSquaredTo(chaser.GlobalPosition) <= closeEnough;
		if (hasReachedTarget) {
			chaser.startIdling();
			return;
		}

		chaser.moveTowards(target, delta);
		chaser.turnTowardsTarget(target, delta);
	}
}

public class ChaseState : ChaserAI {

	private Node2D player;

	public ChaseState(Node2D player) {
		this.player = player;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		var playerPos = player.GlobalPosition;

		chaser.moveTowards(playerPos, delta);
		chaser.turnTowardsTarget(playerPos, delta);
	}
}

public class SeekState : ChaserAI {

	private const float closeEnough = 50.0f;

	private Vector2 walkTarget;
	private Vector2 lookTarget;

	private float timePassed;

	private float lookTimePassed;

	private RandomNumberGenerator rng = new RandomNumberGenerator();

	public SeekState(Vector2 lastSeen) {
		this.walkTarget = lastSeen;
		this.lookTarget = lastSeen;
		// make the enemy pick new position on first time reaching target
		this.lookTimePassed = ChaserStats.lookTime;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		timePassed += delta;
		if (timePassed >= ChaserStats.seekTime) {
			chaser.startIdling();
			return;
		}


		var hasReachedTarget = chaser.GlobalPosition.DistanceSquaredTo(walkTarget) <= closeEnough;
		if (!hasReachedTarget) {
			chaser.moveTowards(walkTarget, delta);
			chaser.turnTowardsTarget(lookTarget, delta);
			return;
		}

		lookTimePassed += delta;
		if (lookTimePassed < ChaserStats.lookTime) {
			// haven't spend enough time looking around -> keep looking
			chaser.turnTowardsTarget(lookTarget, delta);
			return;
		}


		var pickNewWalkTarget = rng.RandiRange(0, ChaserStats.pickNewWalkTargetWeight) == 0;
		if (pickNewWalkTarget) {
			var (min, max) = ChaserStats.howFarNewTargetShouldBe;
			var randomPos = Util.randomVector(rng, min, max);
			var newTarget = chaser.GlobalPosition + randomPos;
			walkTarget = newTarget;
			lookTarget = newTarget;
		} else {
			var randomPos = Util.randomVector(rng, minDistance: 100.0f, maxDistance: 100.0f);
			var newTarget = chaser.GlobalPosition + randomPos;
			lookTarget = newTarget;
			lookTimePassed = 0.0f;
		}
	}
}
