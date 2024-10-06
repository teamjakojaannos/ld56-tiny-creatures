using Godot;

public partial class DialogueOption : Control {
	[Export]
	public Label? SelectionIndicator;

	[Export]
	public Label? OptionIndexLabel;

	[Export]
	public Label? TextContentLabel;

	[Export]
	public LabelSettings? LabelSettings;

	public InteractiveDialogueRow? Row;

	private int _optionIndex = 0;

	public int OptionIndex {
		get => _optionIndex;
		set {
			_optionIndex = value;
			if (OptionIndexLabel is not null) {
				OptionIndexLabel.Text = $"{value + 1}:";
			}
		}
	}

	public Color LabelColor {
		set {
			if (LabelSettings is not null) {
				LabelSettings.FontColor = value;
			}
		}
	}

	public string Text {
		set {
			if (TextContentLabel is not null) {
				TextContentLabel.Text = value;
			}
		}
	}

	private bool isHovered = false;

	public override void _Ready() {
		base._Ready();

		if (TextContentLabel is not null) {
			TextContentLabel.LabelSettings = LabelSettings;
		}

		if (SelectionIndicator is not null) {
			SelectionIndicator.LabelSettings = LabelSettings;
		}

		if (OptionIndexLabel is not null) {
			OptionIndexLabel.LabelSettings = LabelSettings;
		}

		MouseEntered += OnMouseHover;
		MouseExited += OnMouseLeave;
	}

	private void OnMouseHover() {
		Row?.HighlightOption(OptionIndex);
		Select();
		isHovered = true;
	}

	private void OnMouseLeave() {
		isHovered = false;
		Deselect();
	}

	public override void _GuiInput(InputEvent @event) {
		base._GuiInput(@event);

		if (@event.IsActionPressed("gui_click") && isHovered) {
			var dialogue = Dialogue.Instance(this);
			dialogue.SelectOption(OptionIndex);
		}
	}

	public void Select() {
		if (SelectionIndicator is not null) {
			SelectionIndicator.Text = ">";
		}
	}

	public void Deselect() {
		if (SelectionIndicator is not null) {
			SelectionIndicator.Text = " ";
		}
	}

	public void Deactivate() {
		if (SelectionIndicator is not null) {
			SelectionIndicator.Text = " ";
			MouseEntered -= OnMouseHover;
			MouseExited -= OnMouseLeave;
		}
	}
}
