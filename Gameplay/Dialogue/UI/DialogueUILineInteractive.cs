using System;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUILineInteractive : DialogueUILine {
	public int HighlightedOption { get; set; } = 0;
	public int LastOptionIndex => OptionCount - 1;
	public int OptionCount => throw new NotImplementedException();

	public void LockSelection() {
		throw new NotImplementedException();
	}
}
