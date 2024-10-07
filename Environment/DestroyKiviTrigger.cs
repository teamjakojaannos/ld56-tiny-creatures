using Godot;

public partial class DestroyKiviTrigger : Area2D {

	[Export]
	public Node2D? objectToDestroy;

	[Export]
	public DialogueTree? DialogueTree;

	[Export]
	public AudioStreamPlayer? sfx;

	public void bodyEntered(Node2D node) {
		if (node is Player) {
			objectToDestroy?.QueueFree();

			if (DialogueTree is not null) {
				Dialogue.Instance(this).StartDialogue(DialogueTree);
			}

			sfx?.Play();
		}
	}
}
