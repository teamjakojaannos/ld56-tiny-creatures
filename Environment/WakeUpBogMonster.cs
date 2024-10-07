using Godot;

public partial class WakeUpBogMonster : Node2D {

	[Export]
	public BogMonster? monster;


	public override void _Ready() {
		var parent = GetParent<WispInteractable>();
		parent.InteractStart += () => {
			Dialogue.Instance(this).DialogueFinished += OnDialogueFinished;
		};
		parent.InteractStop += () => {
			Dialogue.Instance(this).DialogueFinished -= OnDialogueFinished;
		};
	}

	private void OnDialogueFinished() {
		var currentPosition = monster!.ProgressRatio;
		monster.emergeFromWaterAtPosition(currentPosition);
	}
}
