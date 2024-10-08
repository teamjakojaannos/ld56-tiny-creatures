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

    [Export]
    public string SetStateAfterDialogueEnd = "";

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
                var parent = GetParent<WispInteractable>();
                if (SetStateAfterDialogueEnd == "viineri" && parent is not null && parent.isWispInteracting) {
                    GD.Print("Starting early credits sequence");
                    this.Jukebox().SwitchTrack(Jukebox.MuzakTrack.Credits);
                    this.Persistent().Player!.IsInCinematic = true;
                    this.Persistent().Player!.Animation!.Play("pan_camera_up");
                }

                Dialogue.Instance(this).StartDialogue(DialogueTree);
                Dialogue.Instance(this).DialogueFinished += DialogueFinished;
            }

            isFirstTime = false;
        };
        parent.InteractStop += () => {
            Dialogue.Instance(this).DialogueFinished -= DialogueFinished;
        };
    }

    private void DialogueFinished() {
        if (SetStateAfterDialogueEnd is not null && SetStateAfterDialogueEnd.Trim().Length > 0) {
            this.Persistent().State.Add(SetStateAfterDialogueEnd);

            var parent = GetParent<WispInteractable>();
            if (SetStateAfterDialogueEnd == "viineri" && parent is not null && parent.isWispInteracting) {
                if (GetTree().GetFirstNodeInGroup("ViineriTpTarget") is Node2D win) {
                    this.Persistent().Intro!.FadeToBlack();
                    this.Jukebox().SwitchTrack(Jukebox.MuzakTrack.Credits);

                    GetTree().CreateTimer(3.0f).Timeout += () => {
                        this.Persistent().Player!.TeleportTo(win);
                        this.Persistent().Intro!.FadeInAfterWin();
                        this.Persistent().Player!.IsInCinematic = false;
                    };
                }
            }
        }
    }
}
