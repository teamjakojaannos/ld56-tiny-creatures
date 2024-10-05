public abstract class BogMonsterAIState {
	public abstract void doUpdate(BogMonster monster, float delta);
}

public class MovementState : BogMonsterAIState {

	private bool direction;

	private float speed;

	public MovementState(bool goingForward, float speed) {
		this.direction = goingForward;
		this.speed = speed;
	}

	public override void doUpdate(BogMonster monster, float delta) {
		var sign = direction ? 1.0f : -1.0f;
		var movement = sign * speed * delta;

		monster.Progress += movement;

		var reachedEnd = direction
			? monster.ProgressRatio >= 1.0f
			: monster.ProgressRatio <= 0.0f;

		if (reachedEnd) {
			endOfLine(monster);
		}
	}

	private void endOfLine(BogMonster monster) {
		direction = !direction;
	}
}


