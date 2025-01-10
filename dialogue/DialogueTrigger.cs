using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;

public partial class DialogueTrigger : Area2D {
	[Export]
	public DialogueTree? DialogueContent;

	public override void _Ready() {
		if (DialogueContent is null) {
			return;
		}

		BodyEntered += (body) => {
			if (body is not Player player) {
				return;
			}

			var dialogue = Dialogue.Instance(this);
			dialogue.StartDialogue(DialogueContent);
		};
	}
}
