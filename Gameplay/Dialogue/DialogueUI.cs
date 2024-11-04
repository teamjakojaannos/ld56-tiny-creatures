using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

public partial class DialogueUI : Control {
	[Export]
	[ExportGroup("Config")]
	public int MaxVisibleRows { get; set; } = 3;

	[Export]
	public float OpacityOffsetPerRow { get; set; } = 0.25f;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Control DialogueList {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueList);
		set => this.SetExportProperty(ref _dialogueList, value);
	}
	private Control? _dialogueList;

	[Export]
	[MustSetInEditor]
	public PackedScene DialogueRow {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueRow);
		set => this.SetExportProperty(ref _dialogueRow, value);
	}
	private PackedScene? _dialogueRow;

	[Export]
	[MustSetInEditor]
	public PackedScene InteractiveDialogueRow {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_interactiveDialogueRow);
		set => this.SetExportProperty(ref _interactiveDialogueRow, value);
	}
	private PackedScene? _interactiveDialogueRow;

	[Export]
	[MustSetInEditor]
	public AnimationPlayer Animation {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animation);
		set => this.SetExportProperty(ref _animation, value);
	}
	private AnimationPlayer? _animation;

	public DialogueUIRow? CurrentRow { get; internal set; }

	[Signal]
	public delegate void ClosedEventHandler();

	[Signal]
	public delegate void OpenedEventHandler();

	public override void _Ready() {
		base._Ready();

		if (_animation is not null) {
			Animation.AnimationFinished += (animation) => {
				if (animation == "close") {
					OnClosed();
				} else if (animation == "open") {
					OnOpened();
				}
			};
		}

		Visible = false;
		Clear();
	}

	private void OnOpened() {
		EmitSignal(SignalName.Opened);
	}

	private void OnClosed() {
		Visible = false;
		Clear();

		EmitSignal(SignalName.Closed);
	}

	private void Clear() {
		var dialogueRows = DialogueList
			.GetChildren()
			.OfType<DialogueUIRow>();

		foreach (var row in dialogueRows) {
			DialogueList.RemoveChild(row);
			row.QueueFree();
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (CurrentRow is null) {
			return;
		}

		if (CurrentRow is InteractiveDialogueUIRow row) {
			for (var i = 0; i < 3; i++) {
				var action = $"dialogue_option_{i + 1}";
				if (Input.IsActionJustPressed(action)) {
					row.HighlightedOption = i;
				}
			}

			if (Input.IsActionJustPressed("gui_up")) {
				var option = row.HighlightedOption - 1;
				if (option < 0) {
					option = row.LastOptionIndex;
				}

				row.HighlightedOption = option;
			}

			if (Input.IsActionJustPressed("gui_down")) {
				var option = row.HighlightedOption + 1 % row.OptionCount;
				row.HighlightedOption = option;
			}
		}

		if (@event.IsActionPressed("gui_accept")) {
			if (!CurrentRow.IsFullyVisible) {
				CurrentRow.SkipEntryAnimation();
			} else {
				NextDialogueLine();
			}
		}
	}

	private void NextDialogueLine() {
		if (CurrentRow is InteractiveDialogueUIRow row) {
			var optionIndex = row.HighlightedOption;
			row.SelectOption();
			this.DialogueManager().NextRowForOption(optionIndex);
		} else {
			this.DialogueManager().NextRow();
		}
	}
}
