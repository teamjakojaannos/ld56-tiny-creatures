using System;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Characters;

[Tool]
[GlobalClass]
public partial class DialogueTree : Resource {
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
