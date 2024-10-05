using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class DialogueTree : Resource {
    [Export]
    public bool IsInteractive = false;

    [Export]
    public bool IsLeft = true;

    [Export]
    public Array<string> Lines = new(new[] { "Oispa kaljaa" });

    [Export]
    public Texture2D? Portrait;

    [Export]
    public DialogueTree? Next;

    [Export]
    public DialogueTree? Next2;

    [Export]
    public DialogueTree? Next3;
}
