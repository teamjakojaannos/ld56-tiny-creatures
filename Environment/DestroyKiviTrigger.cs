using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;

public partial class DestroyKiviTrigger : Area2D {

	private bool isDestroyed;

	[Export]
	public Node2D? objectToDestroy;

	[Export]
	public DialogueTree? DialogueTree;

	[Export]
	public AudioStreamPlayer? sfx;

	public void HandleBodyEntered(Node2D node) {
		if (node is PlayerCharacter) {
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
