using Godot;

[Tool]
[GlobalClass]
public partial class GameCharacter : Resource {
	public enum DialogueSide {
		Left,
		Right
	}

	[Export]
	public string Name = "???";

	[Export]
	public DialogueSide DefaultDialogueSide = DialogueSide.Left;

	[Export]
	public DialogueSide PortraitFacing = DialogueSide.Left;

	[Export]
	public PackedScene? Portrait;
}
