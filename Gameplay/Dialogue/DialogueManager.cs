using System;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

using Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

public static class DialogueNodeExtension {
	private static DialogueManager? _instance;
	public static DialogueManager DialogueManager(this Node node) {
		return _instance ??= node.GetTree().Root.GetNode<DialogueManager>("/root/DialogueManager");
	}
}

public partial class DialogueManager : Node {
	[Export]
	public Dialogue? ActiveDialogue { get; set; }

	public bool IsActive => ActiveDialogue != null;

	[Export]
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

	public override void _Ready() {
		base._Ready();
	}

	public void StartDialogue() {
		throw new NotImplementedException();
	}

	public void NextRowForOption(int index) {
		throw new NotImplementedException();
	}

	public void NextRow() {
		throw new NotImplementedException();
	}
}
