using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class DialogueChoiceLine : DialogueLine {
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

	public readonly struct Option {
		public readonly string Text { get; init; }
	}
}