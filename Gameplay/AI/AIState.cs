using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

public partial class AIState : Node {
	private Dictionary<StringName, Variant> _state = [];

	public void SetState(StringName field, Variant? value) {
		if (value is not null) {
			_state[field] = (Variant)value;
		} else {
			_state.Remove(field);
		}
	}

	public Variant? GetState(StringName field) {
		if (_state.ContainsKey(field)) {
			return _state[field];
		}

		return null;
	}
}
