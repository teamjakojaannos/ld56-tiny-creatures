using Godot;

public partial class MyEvilTrigger : Area2D {

	[Export]
	public BogMonster? monster;

	private Timer? first;
	private Timer? second;

	public override void _Ready() {
		first = GetNode<Timer>("Timer1");
		second = GetNode<Timer>("Timer2");

		first.Timeout += goUnderwater;
		second.Timeout += emerge;

		BodyEntered += onBodyEnter;

		// make monster be underwater at start and not override the animation
		monster!.playEmergeFromWaterAnimationThisIsVeryHackyThingDontUse();
		if (monster.ai is BogMonsterStuff.WaitUntilTriggerIsTriggeredState trigger) {
			trigger.animationSet = true;
		}
	}

	public void onBodyEnter(Node2D node) {
		if (node is Player) {
			startSequence();
		}
	}

	private void startSequence() {
		first!.Start();
		second!.Start();
	}

	private void goUnderwater() {
		monster!.playGoUnderwaterAnimationThisIsVeryHackyThingDontUse();
	}

	private void emerge() {
		var currentPosition = monster!.ProgressRatio;
		monster.emergeFromWaterAtPosition(currentPosition);
	}
}
