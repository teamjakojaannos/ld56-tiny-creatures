using System;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

public partial class InteractiveDialogueUIRow : DialogueUIRow {
	public int HighlightedOption { get; set; } = 0;
	public int LastOptionIndex => OptionCount - 1;
	public int OptionCount => throw new NotImplementedException();

	public void SelectOption() {
		throw new NotImplementedException();
	}
}
