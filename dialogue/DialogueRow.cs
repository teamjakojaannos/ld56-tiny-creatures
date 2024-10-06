using Godot;

public partial class DialogueRow : HBoxContainer {
    [Export]
    public bool SpeakerIsOnLeft { get; set; } = true;

    [Export]
    public bool PortraitIsFlippedOnLeft { get; set; } = false;

    [Export]
    public string Text {
        set {
            if (TextContent is not null) {
                TextContent.Text = value;
            }
        }
        get => TextContent?.Text ?? "";
    }

    public virtual bool IsReady => TextContent!.VisibleCharacters == Text.Length;

    [Export]
    [ExportCategory("Prewire")]
    public Label? TextContent;

    [Export]
    public TextureRect? Portrait;

    [Export]
    public Control? PortraitFrame;

    protected virtual bool AutoplayAudio() {
        return true;
    }

    public void StartDialogue() {
        Refresh();

        if (GetNodeOrNull<Timer>("TextScrollTimer") is Timer timer && !Engine.IsEditorHint()) {
            var audio = GetNodeOrNull<AudioStreamPlayer>("SpeakingSfx");
            if (AutoplayAudio()) {
                audio?.Play();
            }

            TextContent!.VisibleCharacters = 0;
            TextContent!.CustomMinimumSize = new(TextContent.Size.X, TextContent.Size.Y);
            timer.Timeout += () => {
                if (IsReady) {
                    timer.Stop();
                    audio?.Stop();
                    return;
                }

                TextContent!.VisibleCharacters++;
            };

            timer.Start();
        }
    }

    private void Refresh() {
        if (Portrait is not null) {
            Portrait.FlipH = SpeakerIsOnLeft == PortraitIsFlippedOnLeft;
        }

        Alignment = SpeakerIsOnLeft
            ? AlignmentMode.Begin
            : AlignmentMode.End;
        if (TextContent is not null) {
            TextContent.SizeFlagsHorizontal = SpeakerIsOnLeft
                ? SizeFlags.ShrinkBegin
                : SizeFlags.ShrinkEnd;
        }

        if (SpeakerIsOnLeft) {
            var firstChildIndex = 0;
            MoveChild(PortraitFrame, firstChildIndex);
        } else {
            var lastChildIndex = GetChildCount() - 1;
            MoveChild(PortraitFrame, lastChildIndex);
        }
    }
}
