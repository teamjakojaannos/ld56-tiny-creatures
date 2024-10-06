using Godot;

public partial class NakkiAttack : Node2D {
	[Export]
	public Area2D? DangerZone;

	public bool IsPlayerInDanger { get; private set; } = false;

	public override void _Ready() {
		base._Ready();

		DangerZone!.BodyEntered += (body) => {
			if (body is Player player) {
				IsPlayerInDanger = true;
			}
		};

		DangerZone!.BodyExited += (body) => {
			if (body is Player player) {
				IsPlayerInDanger = false;
			}
		};
	}
}
