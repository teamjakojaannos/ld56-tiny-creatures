using System;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

using Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;
using System.Linq;

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
		ActiveDialogue ??= GetChildren().OfType<Dialogue>().FirstOrDefault();
		_dialogueUI ??= GetChildren().OfType<DialogueUI>().FirstOrDefault();

		_readyCalled = true;

		if (Engine.IsEditorHint()) {
			return;
		}

		base._Ready();
	}

	private static readonly string[] s_lines = [
		"This line should no longer be visible when the last line is.",
		"A quick brown fox jumps over a lazy dog.",
		"Umm... Huh?",
		"Oh, this is just a placeholder, you silly!"
	];
	private uint _currentLine = 0;

	public void Reset() {
		DialogueUI.Reset();
	}

	public void StartDialogue() {
		Reset();

		DialogueUI.StartDialogue();

		_currentLine = 0;
		var firstLine = s_lines[_currentLine];
		DialogueUI.AddLine(firstLine);
	}

	public void FinishDialogue() {
		DialogueUI.FinishDialogue();
	}

	public void NextRowForOption(int index) {
		throw new NotImplementedException();
	}

	public void NextLine(int _) {
		_currentLine++;
		if (_currentLine >= s_lines.Length) {
			FinishDialogue();
			return;
		}

		var nextLine = s_lines[_currentLine];
		DialogueUI.AddLine(nextLine);
	}
}
