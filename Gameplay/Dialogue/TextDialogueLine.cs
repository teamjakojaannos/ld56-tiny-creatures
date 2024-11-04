using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class TextDialogueLine : DialogueLine {
	[Export]
	public string Text { get; set; } = "Just some yapping. Blablablabla. Etc.";

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