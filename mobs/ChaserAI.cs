using Godot;

namespace ChaserStuff;

public static class ChaserStats {
	// idle for this many seconds, then change state
	public const float idleTime = 5.0f;
	public const float turnFrequency = 1.5f;
	public const float turnChance = 0.25f;

	// wander for this many seconds, then change state
	public const float wanderTime = 15.0f;

	// seek for player this many seconds, then change state
	public const float seekTime = 15.0f;

	// time spent looking around before turning head
	public const float lookTime = 3.0f;

	// motivation to name things properly ends here
	public static (float, float) wanderToNewTargetInRange = (100.0f, 500.0f);

	// update chase target position every x seconds
	public const float chaseTargetPositionUpdateFrequency = 0.2f;

	public const float keepLookingChance = 0.25f;
	public static (float, float) seekNewLocationInRange = (10.0f, 200.0f);
}


public abstract class ChaserAI {
	public abstract void doUpdate(Chaser chaser, float delta);
}

public class IdleState : ChaserAI {

	private float timePassed;
	private float timeSinceLastTurn;

	public override void doUpdate(Chaser chaser, float delta) {
		timePassed += delta;
		timeSinceLastTurn += delta;



		if (timePassed >= ChaserStats.idleTime) {
			chaser.startWandering();
			return;
		}

		if (timeSinceLastTurn >= ChaserStats.turnFrequency) {
			timeSinceLastTurn = 0.0f;
			if (chaser.rng.DiceRoll(ChaserStats.turnChance)) {
				var randomPos = chaser.rng.RandomVector(minDistance: 100.0f, maxDistance: 100.0f);
				var newTarget = chaser.GlobalPosition + randomPos;
				chaser.setLookTarget(newTarget);
			}
		}
	}
}

public class WanderState : ChaserAI {
	private Vector2 target;
	private float timePassed;

	private bool lookTargetSet;
	private bool moveTargetSet;


	public WanderState(Vector2 target) {
		this.target = target;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		if (!lookTargetSet) {
			lookTargetSet = true;
			chaser.setLookTarget(target);
		}

		timePassed += delta;
		if (timePassed >= ChaserStats.wanderTime) {
			chaser.startIdling();
			return;
		}

		if (!chaser.isDoneTurning()) {
			return;
		}

		if (!moveTargetSet) {
			moveTargetSet = true;
			chaser.setMovementTarget(target);
			// clear look target so we look towards movement
			chaser.clearLookTarget();
		}
	}
}

public class ChaseState : ChaserAI {

	private float timeSinceLastUpdate;
	private Node2D player;
	public Vector2 lastSeenPosition;

	public ChaseState(Node2D player) {
		this.player = player;
		timeSinceLastUpdate = ChaserStats.chaseTargetPositionUpdateFrequency;
		lastSeenPosition = player.GlobalPosition;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		var playerPos = player.GlobalPosition;

		chaser.setLookTarget(playerPos, turnInstantly: true);

		timeSinceLastUpdate += delta;
		if (timeSinceLastUpdate >= ChaserStats.chaseTargetPositionUpdateFrequency) {
			timeSinceLastUpdate = 0.0f;
			chaser.setMovementTarget(playerPos);
			lastSeenPosition = playerPos;
		}
	}
}

public class SeekState : ChaserAI {
	private Vector2 lastSeen;
	private bool isTargetSet;

	private float timePassed;

	private float lookTimePassed;

	public SeekState(Vector2 lastSeen) {
		this.lastSeen = lastSeen;
		this.lookTimePassed = 0.0f;
	}

	public override void doUpdate(Chaser chaser, float delta) {
		timePassed += delta;
		if (timePassed >= ChaserStats.seekTime) {
			chaser.startIdling();
			return;
		}

		if (!isTargetSet) {
			isTargetSet = true;
			chaser.setLookTarget(lastSeen);
			chaser.setMovementTarget(lastSeen);
		}

		if (chaser.hasReachedMovementTarget()) {
			lookTimePassed += delta;
		}

		var shouldRollNewAction = lookTimePassed >= ChaserStats.lookTime;
		if (!shouldRollNewAction) {
			return;
		}

		lookTimePassed = 0.0f;

		var pickNewMoveTarget = chaser.rng.DiceRoll(ChaserStats.keepLookingChance);
		if (pickNewMoveTarget) {
			var randomPos = chaser.rng.RandomVector(ChaserStats.seekNewLocationInRange);
			var newTarget = chaser.GlobalPosition + randomPos;
			chaser.setLookTarget(newTarget, turnInstantly: true);
			chaser.setMovementTarget(newTarget);

		} else {
			var randomPos = RandomNumberGeneratorExtension.RandomVector(chaser.rng, minDistance: 100.0f, maxDistance: 100.0f);
			var newTarget = chaser.GlobalPosition + randomPos;
			chaser.setLookTarget(newTarget);
		}
	}
}
