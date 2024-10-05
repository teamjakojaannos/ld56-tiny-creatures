using Godot;

public partial class DialogueOption : Label {
	public InteractiveDialogueRow? Row;
	public int OptionIndex;

	private bool isHovered = false;

	public override void _Ready() {
		base._Ready();

		MouseEntered += () => {
			Row?.HighlightOption(OptionIndex);
			isHovered = true;
		};
		MouseExited += () => isHovered = false;
	}

	public override void _GuiInput(InputEvent @event) {
		base._GuiInput(@event);

		if (@event.IsActionPressed("gui_click") && isHovered) {
			var dialogue = Dialogue.Instance(this);
			dialogue.SelectOption(OptionIndex);
		}
	}
}
