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

    [Export]
    [ExportCategory("Prewire")]
    public Label? TextContent;

    [Export]
    public TextureRect? Portrait;

	public override void _Ready() {
		base._Ready();

        Refresh();
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
