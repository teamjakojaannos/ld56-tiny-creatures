using Godot;

[Tool]
public partial class DialogueRow : HBoxContainer {
    private bool _speakerIsOnLeft;

    [Export]
    public bool SpeakerIsOnLeft {
        set {
            _speakerIsOnLeft = value;
            Refresh();
        }
        get => _speakerIsOnLeft;
    }

    [Export]
    public string Text {
        set {
            if (TextContent is not null) {
                TextContent.Text = value;
            }
        }
        get => TextContent?.Text ?? "";
    }

    public bool IsReady => Text.Length == FullText.Length;

    [Export]
    [ExportCategory("Prewire")]
    public Label? TextContent;

    [Export]
    public TextureRect? Portrait;

    public string FullText = "";

    protected virtual bool AutoplayAudio() {
        return true;
    }

    public override void _Ready() {
        base._Ready();
        Refresh();

        if (GetNodeOrNull<Timer>("TextScrollTimer") is Timer timer && !Engine.IsEditorHint()) {
            var audio = GetNodeOrNull<AudioStreamPlayer>("SpeakingSfx");
            if (AutoplayAudio()) {
                audio?.Play();
            }

            timer.Timeout += () => {
                if (IsReady) {
                    timer.Stop();
                    audio?.Stop();
                    return;
                }

                Text = FullText.Left(Text.Length + 1);
            };

            timer.Start();
        }
    }

    private void Refresh() {
        if (Portrait is null) {
            return;
        }

        Portrait.FlipH = SpeakerIsOnLeft;
        LayoutDirection = SpeakerIsOnLeft
            ? LayoutDirectionEnum.Ltr
            : LayoutDirectionEnum.Rtl;
    }
}
