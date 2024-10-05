using BogMonsterStuff;
using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public float speed = 45.0f;

	private Player? player;

	public BogMonsterStuff.BogMonsterAIState ai = new BogMonsterStuff.MovementState(goingForward: true, 0.0f);

	public RandomNumberGenerator rng = new();

	private AnimationPlayer? animationPlayer;
	private Timer? underwaterCooldown;

	public override void _Ready() {
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		underwaterCooldown = GetNode<Timer>("UnderwaterCooldown");

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

	public bool rollToGoUnderwater(float chance) {
		if (!canGoUnderwater()) {
			return false;
		}

		var goUnderwater = rng.Randf() < BogMonsterStats.goUnderwaterChance;
		if (goUnderwater) {
			this.goUnderwater();
			return true;
		}

		return false;
	}

	public void goUnderwater() {
		var (min, max) = BogMonsterStats.underwaterTime;
		var underwaterTime = rng.RandfRange(min, max);
		ai = new BogMonsterStuff.UnderwaterState(underwaterTime);
		animationPlayer?.Play("go_underwater");
		underwaterCooldown?.Start();
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

	public void emergeFromWaterNearPlayer() {
		float ratio = getPositionRatioThingyThatIsNearestToPlayer() ?? rng.Randf();
		emergeFromWaterAtPosition(ratio);
	}

	private float? getPositionRatioThingyThatIsNearestToPlayer() {
		if (GetParentOrNull<Path2D>() is not Path2D parent) {
			return null;
		}

		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return null;
		}

		var playerPosition = player.GlobalPosition;

		var points = parent.Curve.GetBakedPoints();
		var first = points[0] + parent.GlobalPosition;
		var last = points[points.Length - 1] + parent.GlobalPosition;

		// we only care about X coordinates as bog guy moves on X-axis
		// also I assume the curve goes left-to-right
		// so basically we're trying to find where player's position fits
		// between "first and last nodes in path"

		var smallest_x = Mathf.Min(first.X, last.X);
		var largest_x = Mathf.Max(first.X, last.X);
		var length = Mathf.Abs(largest_x - smallest_x);

		if (length == 0.0f) {
			return null;
		}

		var progress = (playerPosition.X - smallest_x) / length;
		return Mathf.Clamp(progress, 0.0f, 1.0f);
	}

	public void emergefromWaterAnimationDone() {
		var goingForward = Util.randomBool(rng);
		ai = new MovementState(goingForward, speed);
	}

	public bool canGoUnderwater() {
		return underwaterCooldown?.IsStopped() ?? true;
	}
}
