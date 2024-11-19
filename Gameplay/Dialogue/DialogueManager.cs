using System;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

using Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;
using System.Linq;
using System.Collections.Generic;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

public static class DialogueNodeExtension {
	private static DialogueManager? s_instance;
	public static DialogueManager DialogueManager(this Node node) {
		return s_instance ??= node.GetTree().Root.GetNode<DialogueManager>("/root/DialogueManager");
	}
}

[Tool]
public partial class DialogueManager : Node {
	[Export]
	public Dialogue? ActiveDialogue { get; set; }

	public bool IsActive => ActiveDialogue != null;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public DialogueUI DialogueUI {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_dialogueUI);
		set => this.SetExportProperty(ref _dialogueUI, value);
	}
	private DialogueUI? _dialogueUI;

	private List<DialogueLine> _dialogueLines = [];
	private uint _currentLine = 0;

	[Signal]
	public delegate void DialogueOpenedEventHandler();

	[Signal]
	public delegate void DialogueFinishedEventHandler();

	[Signal]
	public delegate void DialogueProgressedEventHandler(string? lineId);

	[Signal]
	public delegate void DialogueOptionSelectedEventHandler(string? lineId, string optionId);

	// HACK: used only for the hacked checkbox-property-buttons, remove alongside those
	private bool _readyCalled = false;

	// FIXME: these can be changed to inspector buttons once Godot 4.4 lands
	[Export]
	[ExportCategory("Controls")]
	public bool Control_Reset {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			CallDeferred(MethodName.Reset);
		}
	}

	[Export]
	public bool Control_Open {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			CallDeferred(MethodName.StartDialogue);
		}
	}

	[Export]
	public bool Control_Next {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			CallDeferred(MethodName.NextLine, -1);
		}
	}

	public override void _Ready() {
		_readyCalled = true;

		ActiveDialogue ??= GetChildren().OfType<Dialogue>().FirstOrDefault();
		_dialogueUI ??= GetChildren().OfType<DialogueUI>().FirstOrDefault();

		if (_dialogueUI is not null) {
			if (!DialogueUI.IsConnected(DialogueUI.SignalName.Opened, Callable.From(ShowFirstLine))) {
				DialogueUI.Opened += ShowFirstLine;
			}
		}

		if (Engine.IsEditorHint()) {
			return;
		}

		base._Ready();
	}

	public void Reset() {
		_dialogueLines.Clear();
		DialogueUI.Reset();
	}

	public void StartDialogue() {
		if (ActiveDialogue is null) {
			GD.PrintErr("No active dialogue to start!");
			return;
		}

		Reset();
		_dialogueLines.AddRange(ActiveDialogue.Lines);
		_currentLine = 0;
		DialogueUI.StartDialogue();
	}

	public void FinishDialogue() {
		DialogueUI.FinishDialogue();
	}

	public void NextRowForOption(int index) {
		throw new NotImplementedException();
	}

	private void ShowFirstLine() {
		_currentLine = 0;
		NextLine(-1);
	}

	public void NextLine(int _) {
		if (!DialogueUI.IsIdle) {
			DialogueUI.SkipCurrentAnimation();
			return;
		}

		if (_currentLine >= _dialogueLines.Count) {
			FinishDialogue();
			return;
		}

		var nextLine = _dialogueLines[(int)_currentLine];
		_currentLine++;

		if (nextLine is DialogueTextLine textLine) {
			DialogueUI.AddLine(textLine.Text, textLine.Side, textLine.Speaker);
		} else {
			throw new NotImplementedException();
		}
	}
}
