using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class ChoiceDialogueLine : DialogueLine {
	[Export]
	public string Prompt { get; set; } = "Choose one:";

	[Export]
	public Godot.Collections.Array<DialogueOption> Options { get; set; } = [];

	/* --------- */
	// FIXME: are these needed?
	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList() {
		return base._GetPropertyList() ?? [];
	}

	public override Variant _Get(StringName property) {
		return base._Get(property);
	}

	public override bool _Set(StringName property, Variant value) {
		return base._Set(property, value);
	}
	/* --------- */
}