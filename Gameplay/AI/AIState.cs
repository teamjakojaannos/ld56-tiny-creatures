using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

[Tool]
[GlobalClass]
public partial class AIState : Node {
	/// <summary>
	/// Exported to help debugging in editor.
	/// </summary>
	[Export]
	private Godot.Collections.Dictionary<StringName, Variant> State { get; set; } = [];

	public void SetState(StringName field, Variant? value) {
		if (value is not null) {
			State[field] = (Variant)value;
		} else {
			State.Remove(field);
		}
	}

	public Variant? GetState(StringName field) {
		if (State.TryGetValue(field, out var value)) {
			return value;
		}

		return null;
	}
}
