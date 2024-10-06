using System;
using Godot;

public partial class Dialogue : CanvasLayer {
	public static Dialogue Instance(Node node) {
		return node.GetTree().Root.GetNode<Dialogue>("Dialogue");
	}

	[Export]
	public int MaxVisibleRows = 3;

	[Export]
	public float OpacityOffsetPerRow = 0.25f;

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

	[Signal]
	public delegate void DialogueUpdatedEventHandler(string chosenOption);

	[Signal]
	public delegate void DialogueFinishedEventHandler();

	public override void _Ready() {
		base._Ready();

		Clear();
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
			child.QueueFree();
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

			row.ClearOptions();
			row.SetupOptions(InteractiveDialogueRowOption, content.Lines);

			ActiveDialogueRow = row;
		} else {
			if (DialogueRow is null) {
				return;
			}

			var row = DialogueRow.Instantiate<DialogueRow>();

			row.Text = "";
			foreach (var line in content.Lines) {
				row.Text += line;
				row.Text += "\n";
			}
			row.Text = row.Text.Trim();

			ActiveDialogueRow = row;
		}

		if (ActiveDialogueRow is not null && DialogueList is not null) {
			var row = ActiveDialogueRow;
			row.SpeakerIsOnLeft = content.DialogueSide == GameCharacter.DialogueSide.Left;
			row.PortraitIsFlippedOnLeft = content.PortraitFacing == GameCharacter.DialogueSide.Right;
			DialogueList.AddChild(row);

			var portrait = content?.Character?.Portrait;
			if (portrait is not null) {
				if (row.PortraitFrame is not null) {
					row.PortraitFrame.QueueFree();
					row.PortraitFrame = null;
				}

				var portraitFrame = portrait.Instantiate<Control>();
				if (portraitFrame.GetNode<Label>("Name") is Label nameLabel) {
					nameLabel.Text = content?.Character?.Name ?? "???";
				}

				row.PortraitFrame = portraitFrame;
				row.PortraitFrameWrapper!.AddChild(portraitFrame);
				row.PortraitFrameWrapper.MoveChild(portraitFrame, 0);
			}

			row.CallDeferred(MethodName.StartDialogue);
		}
	}

	public void NextDialogue(DialogueTree next) {
		if (DialogueList is null) {
			return;
		}

		StartDialogue(next, false);
		if (DialogueList.GetChildCount() > MaxVisibleRows) {
			var child = DialogueList.GetChild(0);
			DialogueList.RemoveChild(child);
			child.QueueFree();
		}

		var childCount = DialogueList.GetChildCount();
		for (var i = 0; i < childCount; ++i) {
			var child = DialogueList.GetChild<Control>(i);
			var color = child.Modulate;
			var opacity = 1.0f - (childCount - 1 - i) * OpacityOffsetPerRow;
			child.Modulate = new Color(color.R, color.G, color.B, opacity);
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (!IsDialogueActive || ActiveDialogue is null || ActiveDialogueRow is null) {
			return;
		}

		int? selectedOption = null;
		bool isSelectEvent = @event.IsActionPressed("gui_accept");
		bool isAllowedToProgress = ActiveDialogueRow.IsReady;
		if (ActiveDialogueRow is InteractiveDialogueRow row) {
			if (@event.IsActionPressed("dialogue_option_1")) {
				row.HighlightOption(0);
			} else if (@event.IsActionPressed("dialogue_option_2")) {
				row.HighlightOption(1);
			} else if (@event.IsActionPressed("dialogue_option_3")) {
				row.HighlightOption(2);
			} else if (@event.IsActionPressed("gui_up")) {
				var option = row.HighlightedOption - 1;
				if (option < 0) {
					option = row.OptionCount - 1;
				}

				row.HighlightOption(option);
			} else if (@event.IsActionPressed("gui_down")) {
				var option = (row.HighlightedOption + 1) % row.OptionCount;
				row.HighlightOption(option);
			}

			if (isSelectEvent) {
				row.SelectOption(row.HighlightedOption);
				selectedOption = row.HighlightedOption;
			}
		}

		if (isSelectEvent && isAllowedToProgress) {
			SelectOption(selectedOption ?? 0);
		}
	}

	public void SelectOption(int option) {
		if (ActiveDialogue is null) {
			return;
		}

		var next = option switch {
			0 => ActiveDialogue.Next,
			1 => ActiveDialogue.Next2,
			2 => ActiveDialogue.Next3,
			_ => throw new InvalidOperationException("Selected option out of bounds"),
		};

		var selectedOption = ActiveDialogue.Lines[option];
		if (next is null) {
			CloseDialogue();

			EmitSignal(SignalName.DialogueFinished);
		} else {
			NextDialogue(next);
		}

		EmitSignal(SignalName.DialogueUpdated, selectedOption);
	}
}
