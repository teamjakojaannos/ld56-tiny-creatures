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
	public PackedScene DialogueTextLineTemplate {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueLineTemplate);
		set => this.SetExportProperty(ref _dialogueLineTemplate, value);
	}
	private PackedScene? _dialogueLineTemplate;

	[Export]
	[MustSetInEditor]
	public PackedScene DialogueChoiceLineTemplate {
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

	public bool IsFinished => !Visible;

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

	public void AddTextLine(string text, DialogueSide side, GameCharacter? character) {
		var uiLine = DialogueTextLineTemplate.Instantiate<DialogueUITextLine>();
		SetupAddedLine(uiLine, text, side, character);
		RefreshLinePositions();

		uiLine.OnAdded();
	}

	public void AddChoiceLine(DialogueSide side, GameCharacter? character, IEnumerable<DialogueChoiceLine.Option> options) {
		var uiLine = DialogueChoiceLineTemplate.Instantiate<DialogueUIChoiceLine>();
		SetupAddedLine(uiLine, string.Empty, side, character);
		RefreshLinePositions();

		uiLine.ClearOptions();
		uint optionIndex = 0;
		foreach (var option in options) {
			var uiOption = uiLine.OptionTemplate.Instantiate<DialogueUIChoiceLineOption>();
			uiOption.Setup(optionIndex, option);

			uiLine.AddOption(uiOption);
			optionIndex++;
		}
		uiLine.UpdateHighlighted();

		uiLine.OnAdded();
	}

	// FIXME: this should reside in `DialogueUILine` and be overridden by text/choice lines to add specific behaviour, if needed
	private void SetupAddedLine(DialogueUILine uiLine, string text, DialogueSide side, GameCharacter? character) {
		DialogueLines.AddChild(uiLine);
		uiLine.Owner = DialogueLines;
		uiLine.Side = side;

		if (uiLine is DialogueUITextLine textUiLine) {
			textUiLine.Text = text;
		}

		if (character?.Portrait is not null) {
			uiLine.Portrait.Texture = character.Portrait;

			var isFacingWrongWay = uiLine.Side == character.PortraitFacing;
			uiLine.Portrait.FlipH = isFacingWrongWay;
		}

		if (character?.Name is not null) {
			uiLine.CharacterName = character.Name;
		}
	}

	private void RefreshLinePositions() {
		var position = (uint)Lines.Count();
		foreach (var line in Lines) {
			position--;
			line.LinePosition = position;
		}
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
		var lines = Lines.ToList();
		foreach (var row in lines) {
			DialogueLines.RemoveChild(row);
			row.QueueFree();
		}
	}

	public interface DialogueUIInputEvent {
		public sealed class SelectOption(uint option) : DialogueUIInputEvent {
			public uint Option { get; init; } = option;
		}

		public sealed class NextOption : DialogueUIInputEvent;

		public sealed class PreviousOption : DialogueUIInputEvent;

		public sealed class Proceed : DialogueUIInputEvent;
	}

	public override void _Input(InputEvent @event) {
		if (Engine.IsEditorHint()) {
			return;
		}

		base._Input(@event);
		if (CurrentLine is null) {
			return;
		}

		DialogueUIInputEvent? inputEvent = null;
		for (var i = 0; i < 3; i++) {
			var action = $"dialogue_option_{i + 1}";
			if (Input.IsActionJustPressed(action)) {
				inputEvent = new DialogueUIInputEvent.SelectOption((uint)i);
			}
		}

		if (inputEvent is null) {
			if (Input.IsActionJustPressed("gui_up")) {
				inputEvent = new DialogueUIInputEvent.PreviousOption();
			} else if (Input.IsActionJustPressed("gui_down")) {
				inputEvent = new DialogueUIInputEvent.NextOption();
			} else if (@event.IsActionPressed("gui_accept")) {
				inputEvent = new DialogueUIInputEvent.Proceed();
			}
		}

		if (inputEvent is not null) {
			HandleInput(inputEvent);
		}
	}

	public void HandleInput(DialogueUIInputEvent @event) {
		Action handler = @event switch {
			DialogueUIInputEvent.Proceed => NextDialogueLine,
			DialogueUIInputEvent.NextOption => NextDialogueOption,
			DialogueUIInputEvent.PreviousOption => PreviousDialogueOption,
			DialogueUIInputEvent.SelectOption selectEvent => () => SelectDialogueOption((int)selectEvent.Option),
			_ => throw new NotImplementedException(),
		};

		handler();
	}

	private void NextDialogueLine() {
		var optionIndex = -1;
		if (CurrentLine is DialogueUIChoiceLine row) {
			optionIndex = row.HighlightedOption;

			if (IsIdle) {
				row.LockSelection();
			}
		}

		// HACK: While in editor, the autoload/singleton is not the same as the
		// instance being edited in the viewport. The UI is rendered fullscreen
		// (whole editor window) instead of as part of the edited scene.
		//
		// To circumvent this, assume the parent is the dialogue manager while
		// inside the editor.
		var manager = Engine.IsEditorHint()
			? GetParentOrNull<DialogueManager>()
			: this.DialogueManager();

		manager.NextLine(optionIndex);
	}

	private void NextDialogueOption() {
		if (CurrentLine is not DialogueUIChoiceLine line) {
			return;
		}

		var option = (line.HighlightedOption + 1) % line.OptionCount;
		line.HighlightedOption = option;
	}

	private void PreviousDialogueOption() {
		if (CurrentLine is not DialogueUIChoiceLine line) {
			return;
		}

		var option = line.HighlightedOption - 1;
		if (option < 0) {
			option = line.LastOptionIndex;
		}

		line.HighlightedOption = option;
	}

	private void SelectDialogueOption(int optionIndex) {
		if (CurrentLine is not DialogueUIChoiceLine line) {
			return;
		}

		line.HighlightedOption = optionIndex;
		NextDialogueLine();
	}
}
