using Godot;

using Jakojaannos.WisperingWoods.Gameplay.Dialogue;

namespace Jakojaannos.WisperingWoods.Characters;

[Tool]
[GlobalClass]
public partial class GameCharacter : Resource {
	[Export]
	public string Name { get; set; } = "???";

	[Export]
	public DialogueSide DefaultDialogueSide { get; set; } = DialogueSide.Left;

	[Export]
	public DialogueSide PortraitFacing { get; set; } = DialogueSide.Left;

	[Export]
	public Texture2D? Portrait { get; set; }
}
