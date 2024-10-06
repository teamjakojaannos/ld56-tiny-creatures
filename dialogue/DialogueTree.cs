using System;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class DialogueTree : Resource {
    public GameCharacter.DialogueSide DialogueSide =>
        OverriddenSide
        ?? Character?.DefaultDialogueSide
        ?? throw new InvalidOperationException("Dialogue tree missing game character information");

    private GameCharacter.DialogueSide? OverriddenSide => SideOverride switch {
        DialogueSideOverride.Left => GameCharacter.DialogueSide.Left,
        DialogueSideOverride.Right => GameCharacter.DialogueSide.Right,
        _ => null
    };

    public GameCharacter.DialogueSide PortraitFacing =>
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
    [ExportGroup("Overrides")]
    public DialogueSideOverride SideOverride;

    public enum DialogueSideOverride {
        None,
        Left,
        Right
    }
}
