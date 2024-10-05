using Godot;

public partial class BogMonster : PathFollow2D {

	[Export]
	public float speed = 60.0f;

	private bool goingForward = true;

	private Player? player;

	public BogMonsterAIState ai = new MovementState(goingForward: true, 0.0f);

	public RandomNumberGenerator rng = new();

	public override void _Ready() {
		ai = new MovementState(goingForward: true, speed);
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
}
