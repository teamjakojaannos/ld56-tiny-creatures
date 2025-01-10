using System;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Characters;
using Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class DialogueTree : Resource {
	public DialogueSide DialogueSide =>
		OverriddenSide
		?? Character?.DefaultDialogueSide
		?? throw new InvalidOperationException("Dialogue tree missing game character information");

	private DialogueSide? OverriddenSide => SideOverride switch {
		DialogueSideOverride.Left => DialogueSide.Left,
		DialogueSideOverride.Right => DialogueSide.Right,
		_ => null
	};

	public DialogueSide PortraitFacing =>
		Character?.PortraitFacing
		?? throw new InvalidOperationException("Dialogue tree missing game character information");

	[Export]
	public GameCharacter? Character;

	[Export]
	public bool IsInteractive = false;

	[Export]
	public Array<string> Lines = new(new[] { "Oispa kaljaa" });

	[Export]
	public DialogueTree? Next;

	[Export]
	public DialogueTree? Next2;

	[Export]
	public DialogueTree? Next3;

	[Export]
	[ExportGroup("Effects")]
	public float ScreenShakeAmount = 0.0f;
	[Export]
	public float ScreenShakeFade = 30.0f;

	[Export]
	[ExportGroup("Overrides")]
	public DialogueSideOverride SideOverride;

	public enum DialogueSideOverride {
		None,
		Left,
		Right
	}
}
