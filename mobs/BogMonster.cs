using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public float speed = 60.0f;

	private bool goingForward = true;

	public override void _Process(double _delta) {
		var delta = (float)_delta;
		var sign = goingForward ? 1.0f : -1.0f;
		var movement = sign * speed * delta;

		Progress += movement;

		var reachedEnd = goingForward
			? ProgressRatio >= 1.0f
			: ProgressRatio <= 0.0f;

		if (reachedEnd) {
			goingForward = !goingForward;
		}
	}


	public void sightConeEntered(Node2D node) {
		if (node is not Player player) {
			return;
		}

		GD.Print("Player spotted!");
	}

	public void sightConeExited(Node2D node) {
		if (node is not Player player) {
			return;
		}

		GD.Print("Player lost");
	}
}
