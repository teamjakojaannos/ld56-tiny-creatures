using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Characters;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUI : CanvasLayer {
	[Export]
	[ExportGroup("Config")]
	public int MaxVisibleRows { get; set; } = 3;

	[Export]
	public float OpacityOffsetPerRow { get; set; } = 0.25f;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Control DialogueLines {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueLines);
		set => this.SetExportProperty(ref _dialogueLines, value);
	}
	private Control? _dialogueLines;

	[Export]
	[MustSetInEditor]
	public PackedScene DialogueLineTemplate {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueLineTemplate);
		set => this.SetExportProperty(ref _dialogueLineTemplate, value);
	}
	private PackedScene? _dialogueLineTemplate;

	[Export]
	[MustSetInEditor]
	public PackedScene InteractiveDialogueLineTemplate {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_interactiveDialogueLineTemplate);
		set => this.SetExportProperty(ref _interactiveDialogueLineTemplate, value);
	}
	private PackedScene? _interactiveDialogueLineTemplate;

	[Export]
	[MustSetInEditor]
	public AnimationPlayer Animation {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animation);
		set => this.SetExportProperty(ref _animation, value);
	}
	private AnimationPlayer? _animation;

	public IEnumerable<DialogueUILine> Lines => DialogueLines
			.GetChildren()
			.Where(child => child.GetType().IsAssignableTo(typeof(DialogueUILine)))
			.Select(child => (child as DialogueUILine)!);

	public DialogueUILine? CurrentLine => Lines.LastOrDefault();

	public bool IsIdle => CurrentLine is null
		? !Animation.IsPlaying()
		: CurrentLine.IsFullyVisible;

	[Signal]
	public delegate void ClosedEventHandler();

	[Signal]
	public delegate void OpenedEventHandler();

	public override void _Ready() {
		_animation ??= GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
		base._Ready();

		if (_animation is not null) {
			Animation.AnimationFinished += (animation) => {
				if (animation == "FinishDialogue") {
					OnClosed();
				} else if (animation == "StartDialogue") {
					OnOpened();
				}
			};
		}

		Visible = false;
		Clear();
	}

	public void Reset() {
		GD.Print("Resetting dialogue");
		Animation.Play("RESET");
		Clear();
	}

	public void StartDialogue() {
		GD.Print("Starting dialogue");
		Clear();

		Animation.Play("StartDialogue");
	}

	public void SkipCurrentAnimation() {
		if (Animation.CurrentAnimation == "StartDialogue") {
			Animation.Seek(Animation.CurrentAnimationLength - 0.1);
		} else {
			CurrentLine?.SkipEntryAnimation();
		}
	}

	public void FinishDialogue() {
		GD.Print("Dialogue finished");

		Animation.Play("FinishDialogue");
	}

	public void AddLine(string text, DialogueSide side, GameCharacter? character) {
		var uiLine = DialogueLineTemplate.Instantiate<DialogueUITextLine>();
		DialogueLines.AddChild(uiLine);
		uiLine.Owner = DialogueLines;
		uiLine.Text = text;
		uiLine.Side = side;

		if (character?.Portrait is not null) {
			uiLine.Portrait.Texture = character.Portrait;

			var isFacingWrongWay = uiLine.Side == character.PortraitFacing;
			uiLine.Portrait.FlipH = isFacingWrongWay;
		}

		var lines = Lines;
		var position = (uint)lines.Count();
		foreach (var line in lines) {
			position--;
			line.LinePosition = position;
		}

		uiLine.OnAdded();
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
		var dialogueRows = DialogueLines
			.GetChildren()
			.Where(child => typeof(DialogueUILine).IsAssignableFrom(child.GetType()));

		foreach (var row in dialogueRows) {
			DialogueLines.RemoveChild(row);
			row.QueueFree();
		}
	}

	public override void _Input(InputEvent @event) {
		if (Engine.IsEditorHint()) {
			return;
		}

		base._Input(@event);
		if (CurrentLine is null) {
			return;
		}

		// FIXME: why this isn't handled in the interactive line itself?
		if (CurrentLine is DialogueUILineInteractive line) {
			for (var i = 0; i < 3; i++) {
				var action = $"dialogue_option_{i + 1}";
				if (Input.IsActionJustPressed(action)) {
					line.HighlightedOption = i;
				}
			}

			if (Input.IsActionJustPressed("gui_up")) {
				var option = line.HighlightedOption - 1;
				if (option < 0) {
					option = line.LastOptionIndex;
				}

				line.HighlightedOption = option;
			}

			if (Input.IsActionJustPressed("gui_down")) {
				var option = line.HighlightedOption + 1 % line.OptionCount;
				line.HighlightedOption = option;
			}
		}

		if (@event.IsActionPressed("gui_accept")) {
			NextDialogueLine();
		}
	}

	private void NextDialogueLine() {
		var optionIndex = -1;
		if (CurrentLine is DialogueUILineInteractive row) {
			optionIndex = row.HighlightedOption;

			if (IsIdle) {
				row.LockSelection();
			}
		}

		this.DialogueManager().NextLine(optionIndex);
	}
}
