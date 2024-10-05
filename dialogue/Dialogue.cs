using System;
using Godot;

public partial class Dialogue : CanvasLayer {
	public static Dialogue Instance(Node node) {
		return node.GetTree().Root.GetNode<Dialogue>("Dialogue");
	}

	[Export]
	public PackedScene? DialogueRow;

	[Export]
	public PackedScene? InteractiveDialogueRow;

	[Export]
	public PackedScene? InteractiveDialogueRowOption;

	[Export]
	public Control? DialogueList;

	private DialogueTree? ActiveDialogue;
	private DialogueRow? ActiveDialogueRow;

	public bool IsDialogueActive => ActiveDialogue != null && Visible;

	public override void _Ready() {
		base._Ready();

		Visible = false;
	}

	public void CloseDialogue() {
		ActiveDialogue = null;
		ActiveDialogueRow = null;

		Clear();
		Visible = false;
	}

	private void Clear() {
		if (DialogueList is null) {
			return;
		}

		foreach (var child in DialogueList.GetChildren()) {
			DialogueList.RemoveChild(child);
		}
	}

	public void StartDialogue(DialogueTree content, bool clear = true) {
		if (DialogueList is null) {
			return;
		}

		ActiveDialogue = content;
		Visible = true;

		if (clear) {
			Clear();
		}

		if (content.IsInteractive) {
			if (InteractiveDialogueRow is null || InteractiveDialogueRowOption is null) {
				return;
			}

			var row = InteractiveDialogueRow.Instantiate<InteractiveDialogueRow>();
			DialogueList.AddChild(row);

			var options = row.GetNode("Options");
			foreach (var child in options.GetChildren()) {
				options.RemoveChild(child);
			}

			foreach (var line in content.Lines) {
				var option = InteractiveDialogueRowOption.Instantiate<Label>();
				option.Text = line;
				options.AddChild(option);
			}

			row.SpeakerIsOnLeft = content.IsLeft;
			row.SetupNumbers();
			row.HighlightOption(0);
			ActiveDialogueRow = row;
		} else {
			if (DialogueRow is null) {
				return;
			}

			var row = DialogueRow.Instantiate<DialogueRow>();
			DialogueList.AddChild(row);

			row.Text = "";
			foreach (var line in content.Lines) {
				row.Text += line;
				row.Text += "\n";
			}
			row.Text = row.Text.Trim();

			row.SpeakerIsOnLeft = content.IsLeft;
			ActiveDialogueRow = row;
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (!IsDialogueActive || ActiveDialogue is null || ActiveDialogueRow is null) {
			return;
		}

		DialogueTree? next = null;
		bool isSelectEvent = @event.IsActionPressed("dialogue_select_option");
		if (ActiveDialogueRow is InteractiveDialogueRow row) {
			if (@event.IsActionPressed("dialogue_option_1")) {
				row.HighlightOption(0);
			} else if (@event.IsActionPressed("dialogue_option_2")) {
				row.HighlightOption(1);
			} else if (@event.IsActionPressed("dialogue_option_3")) {
				row.HighlightOption(2);
			}

			if (isSelectEvent) {
				next = row.HighlightedOption switch {
					0 => ActiveDialogue.Next,
					1 => ActiveDialogue.Next2,
					2 => ActiveDialogue.Next3,
					_ => throw new InvalidOperationException("Selected option out of bounds"),
				};
			}
		} else {
			if (isSelectEvent) {
				next = ActiveDialogue.Next;
			}
		}

		if (isSelectEvent) {
			if (next is null) {
				CloseDialogue();
			} else {
				StartDialogue(next, false);
			}
		}
	}
}
