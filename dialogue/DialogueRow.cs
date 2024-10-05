using Godot;

public partial class DialogueRow : HBoxContainer {
    [Export]
    public bool SpeakerIsOnLeft { get; set; } = true;

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

    [Export]
    public Control? TextContentContainer;

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
        if (Portrait is null) {
            return;
        }

        Portrait.FlipH = SpeakerIsOnLeft;
        Alignment = SpeakerIsOnLeft
            ? AlignmentMode.Begin
            : AlignmentMode.End;
        if (TextContent is not null) {
            TextContent.SizeFlagsHorizontal = SpeakerIsOnLeft
                ? SizeFlags.ShrinkBegin
                : SizeFlags.ShrinkEnd;
        }

        if (TextContentContainer is null) {
            Alignment = AlignmentMode.Begin;
            return;
        }

        RemoveChild(PortraitFrame);
        RemoveChild(TextContentContainer);

        if (SpeakerIsOnLeft) {
            AddChild(PortraitFrame);
            AddChild(TextContentContainer);
        } else {
            AddChild(TextContentContainer);
            AddChild(PortraitFrame);
        }
    }
}
