using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class WispInteractableDialogueTrigger : Node2D {
    private DialogueTree? _dialogueTree;

    [Export]
    public DialogueTree DialogueTree {
        get => _dialogueTree ?? Util.TrustMeBro<DialogueTree>();
        set {
            _dialogueTree = value;
            UpdateConfigurationWarnings();
        }
    }

    [Export]
    public bool OneShot = false;

    private bool isFirstTime = true;

    public override string[] _GetConfigurationWarnings() {
        var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();
        if (GetParentOrNull<WispInteractable>() is null) {
            warnings = warnings.Append("Parent must be a WispInteractable!").ToArray();
        }

        if (_dialogueTree is null) {
            warnings = warnings.Append("DialogueTree is not set!").ToArray();
        }

        return warnings;
    }

    public override void _Ready() {
        base._Ready();

        if (Engine.IsEditorHint()) {
            return;
        }

        var parent = GetParent<WispInteractable>();
        parent.InteractStart += () => {
            if (isFirstTime || !OneShot) {
                Dialogue.Instance(this).StartDialogue(DialogueTree);
            }

            isFirstTime = false;
        };
        parent.InteractStop += () => {};
    }
}
