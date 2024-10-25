using Godot;

public partial class MyEvilTrigger : Area2D {

	[Export]
	public BogMonster? monster;

	private Timer? first;
	private Timer? second;

	public override void _Ready() {
		first = GetNode<Timer>("Timer1");
		second = GetNode<Timer>("Timer2");

		first.Timeout += GoUnderwater;
		second.Timeout += Emerge;

		BodyEntered += OnBodyEnter;

		// make monster be underwater at start and not override the animation
		monster!.PlayEmergeFromWaterAnimationThisIsVeryHackyThingDontUse();
		if (monster.ai is BogMonsterStuff.WaitUntilTriggerIsTriggeredState trigger) {
			trigger.animationSet = true;
		}
	}

	public void OnBodyEnter(Node2D node) {
		if (node is Player) {
			StartSequence();
		}
	}

	private void StartSequence() {
		first!.Start();
		second!.Start();
	}

	private void GoUnderwater() {
		monster!.PlayGoUnderwaterAnimationThisIsVeryHackyThingDontUse();
	}

	private void Emerge() {
		var currentPosition = monster!.ProgressRatio;
		monster.EmergeFromWaterAtPosition(currentPosition);
	}
}
