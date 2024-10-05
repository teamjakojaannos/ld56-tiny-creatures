using BogMonsterStuff;
using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public float speed = 60.0f;

	private Player? player;

	public BogMonsterStuff.BogMonsterAIState ai = new BogMonsterStuff.MovementState(goingForward: true, 0.0f);

	public RandomNumberGenerator rng = new();

	private AnimationPlayer? animationPlayer;

	public override void _Ready() {
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		ai = new BogMonsterStuff.MovementState(goingForward: true, speed);
	}

	public override void _Process(double _delta) {
		var delta = (float)_delta;

		ai.doUpdate(this, delta);
	}


	public void sightConeEntered(Node2D node) {
		if (node is Player player) {
			this.player = player;
		}
	}

	public void sightConeExited(Node2D node) {
		if (node is Player) {
			player = null;
		}
	}

	public void goUnderwater() {
		var (min, max) = BogMonsterStats.underwaterTime;
		var underwaterTime = rng.RandfRange(min, max);
		ai = new BogMonsterStuff.UnderwaterState(underwaterTime);
		animationPlayer?.Play("go_underwater");
	}

	public void goUnderwaterAnimationDone() {
		if (ai is UnderwaterState underwater) {
			underwater.animationDone = true;
		}
	}

	public void emergeFromWaterAtPosition(float progress) {
		ProgressRatio = progress;
		animationPlayer?.Play("emerge_from_water");
	}

	public void emergefromWaterAnimationDone() {
		var goingForward = Util.randomBool(rng);
		ai = new MovementState(goingForward, speed);
	}
}
