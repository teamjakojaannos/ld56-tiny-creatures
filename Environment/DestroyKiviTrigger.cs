using Godot;

public partial class DestroyKiviTrigger : Area2D {

	private bool isDestroyed;

	[Export]
	public Node2D? objectToDestroy;

	[Export]
	public DialogueTree? DialogueTree;

	[Export]
	public AudioStreamPlayer? sfx;

	public void HandleBodyEntered(Node2D node) {
		if (node is Player) {
			if (isDestroyed) {
				return;
			}
			isDestroyed = true;

			objectToDestroy?.QueueFree();

			if (DialogueTree is not null) {
				Dialogue.Instance(this).StartDialogue(DialogueTree);
			}

			sfx?.Play();
		}
	}
}
