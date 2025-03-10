
using Godot;

[Tool]
[GlobalClass]
public partial class BogMonsterStats : Resource {
	[Export]
	public float speed = 45.0f;

	[Export]
	public float detectionGain = 100.0f;
	[Export]
	public float detectionDecay = 60.0f;

	// moves for this many seconds, then rolls to change state
	[Export] public float moveTime = 3.0f;

	// chance to stop movement and change state
	[Export] public float stopMoveChance = 0.40f;

	// chance to stop movement and change state
	[Export] public float changeDirectionInsteadOfStoppingChance = 0.10f;

	[Export] public float goUnderwaterChance = 0.40f;

	// min/max time to stay underwater
	[Export] public float minUnderwaterTime = 1.0f;
	[Export] public float maxUnderwaterTime = 3.0f;

	public (float, float) UnderwaterTime => (minUnderwaterTime, maxUnderwaterTime);

	// min and max idle time
	[Export] public float minIdleTime = 0.5f;
	[Export] public float maxIdleTime = 2.0f;

	public (float, float) IdleTime => (minIdleTime, maxIdleTime);

	[Export] public float emergeAtPlayerChance = 0.35f;
	[Export] public float emergeAtSameLocationChance = 0.0f;

	[Export] public float alertTime = 7.5f;
	[Export] public float alertThreshold = 40.0f;
	[Export] public float attackThreshold = 100.0f;

	// how many seconds until the hand snatches the player
	[Export] public float attackTime = 1.0f;
	[Export] public float attackAnimationSpeed = 1.0f;
	[Export] public float emergeAnimationSpeed = 1.0f;
}
