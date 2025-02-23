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
		return s_instance ??= node.GetTree().Root.GetNode<DialogueManager>("/root/DialogueMan");
	}
}

[Tool]
[GlobalClass]
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
	public bool Control_DebugReset {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			CallDeferred(MethodName.Reset);
		}
	}

	[Export]
	public bool Control_End {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			CallDeferred(MethodName.ShowLastLine);
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

			DebugSendUIInput(new DialogueUI.DialogueUIInputEvent.Proceed());
		}
	}

	[Export]
	public bool Control_PreviousOption {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			DebugSendUIInput(new DialogueUI.DialogueUIInputEvent.PreviousOption());
		}
	}

	[Export]
	public bool Control_NextOption {
		get => false;
		set {
			if (!_readyCalled) {
				return;
			}

			DebugSendUIInput(new DialogueUI.DialogueUIInputEvent.NextOption());
		}
	}

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
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
			// Workaround https://github.com/godotengine/godot/issues/71373
			// TL;DR: we want configuration warnings, without an editor-global
			//        instance.
			//
			// DO NOT FREE THE AUTOLOAD INSTANCE. See Persistent.cs for details
			if (GetViewport() is Window) {
				GetParent().RemoveChild(this);
			}
			return;
		}
	}

	public void Reset() {
		_dialogueLines.Clear();
		DialogueUI.Reset();
	}

	public void ShowLastLine() {
		if (DialogueUI.IsFinished) {
			StartDialogue();
		}

		for (++_currentLine; _currentLine < _dialogueLines.Count; ++_currentLine) {
			ShowLine((int)_currentLine);
			DialogueUI.SkipCurrentAnimation();
		}
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

	public void DebugSendUIInput(DialogueUI.DialogueUIInputEvent @event) {
		DialogueUI.HandleInput(@event);
	}

	public void FinishDialogue() {
		DialogueUI.FinishDialogue();
	}

	private void ShowFirstLine() {
		_currentLine = 0;
		NextLine(-1);
	}

	public void NextLine(int optionIndex) {
		if (DialogueUI.IsFinished || ActiveDialogue is null) {
			return;
		}

		if (!DialogueUI.IsIdle) {
			DialogueUI.SkipCurrentAnimation();
			return;
		}

		if (optionIndex >= 0) {
			var nextDialogueBranch = ActiveDialogue
				.GetChildren()
				.OfType<Dialogue>()
				.ElementAt(optionIndex);

			_dialogueLines.AddRange(nextDialogueBranch.Lines);
			ActiveDialogue = nextDialogueBranch;
		}

		if (_currentLine >= _dialogueLines.Count) {
			FinishDialogue();
			return;
		}

		ShowLine((int)_currentLine);
		_currentLine++;
	}

	private void ShowLine(int lineIndex) {
		if (ActiveDialogue is null) {
			throw new InvalidOperationException($"Cannot show dialogue line {lineIndex}: No active dialogue");
		}
		if (lineIndex < 0 || lineIndex >= _dialogueLines.Count) {
			throw new ArgumentOutOfRangeException($"Cannot show dialogue line {lineIndex}: Index is out of bounds");
		}

		var nextLine = _dialogueLines[lineIndex];

		if (nextLine is DialogueTextLine textLine) {
			DialogueUI.AddTextLine(textLine.Text, textLine.Side, textLine.Speaker);
		} else if (nextLine is DialogueChoiceLine choiceLine) {
			// Mangle dialgoue branches stored as children to more manageable dialogue options
			var options = ActiveDialogue
				.GetChildren()
				.OfType<Dialogue>()
				.Select(branch => new DialogueChoiceLine.Option() {
					Text = GetDialogueBranchFirstLineAsText(branch),
				});
			DialogueUI.AddChoiceLine(choiceLine.Side, choiceLine.Speaker, options);
		} else {
			throw new NotImplementedException();
		}
	}

	private static string GetDialogueBranchFirstLineAsText(Dialogue branch) {
		var firstLine = branch.Lines.First();
		if (firstLine is not DialogueTextLine textLine) {
			// TODO: configuration warning
			throw new InvalidOperationException("First dialogue line of a dialogue branch must be a text line!");
		}

		return textLine.Text;
	}
}
