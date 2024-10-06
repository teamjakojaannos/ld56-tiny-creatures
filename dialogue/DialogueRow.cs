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
    public Control? PortraitFrame;

    [Export]
    public Control? PortraitFrameWrapper;

    [Export]
    public HBoxContainer? Container;

    [Export]
    public Timer? TextScrollTimer;

    [Export]
    public AudioStreamPlayer? SpeakingSfx;

    protected virtual bool AutoplayAudio() {
        return true;
    }

    public void StartDialogue() {
        Refresh();

        if (TextScrollTimer is not null && !Engine.IsEditorHint()) {
            TextContent!.VisibleCharacters = 0;
            TextContent!.CustomMinimumSize = new(TextContent.Size.X, TextContent.Size.Y);
            TextScrollTimer.Timeout += () => {
                if (IsReady) {
                    TextScrollTimer.Stop();
                    SpeakingSfx!.Stop();
                    return;
                }

                TextContent!.VisibleCharacters++;
            };

            GetTree().CreateTimer(0.5f).Timeout += () => {
                TextScrollTimer.Start();
                if (AutoplayAudio()) {
                    SpeakingSfx!.Play();
                }
            };
        }

        PortraitFrame
            ?.GetNode<AnimationPlayer>("AnimationPlayer")
            ?.Play(SpeakerIsOnLeft ? "enter_left" : "enter_right");
    }

    private void Refresh() {
        if (PortraitFrame?.GetNodeOrNull<TextureRect>("PortraitFrame/Character") is TextureRect portrait) {
            portrait.FlipH = SpeakerIsOnLeft == PortraitIsFlippedOnLeft;
        }

        Container!.Alignment = SpeakerIsOnLeft
            ? AlignmentMode.Begin
            : AlignmentMode.End;
        if (TextContent is not null) {
            TextContent.SizeFlagsHorizontal = SpeakerIsOnLeft
                ? SizeFlags.ShrinkBegin
                : SizeFlags.ShrinkEnd;
        }

        if (SpeakerIsOnLeft) {
            var firstChildIndex = 0;
            MoveChild(PortraitFrameWrapper == this ? PortraitFrame : PortraitFrameWrapper, firstChildIndex);
        } else {
            var lastChildIndex = GetChildCount() - 1;
            MoveChild(PortraitFrameWrapper == this ? PortraitFrame : PortraitFrameWrapper, lastChildIndex);
        }
    }
}
