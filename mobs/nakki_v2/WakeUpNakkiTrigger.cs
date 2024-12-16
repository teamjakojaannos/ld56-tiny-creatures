using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class WakeUpNakkiTrigger : Area2D {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? Array.Empty<string>();

		if (_nakkiToTrigger == null) {
			warnings = warnings.Append("Trigger target is not set").ToArray();
		}

		return warnings.ToArray();
	}

	[Export] private NakkiV2? _nakkiToTrigger;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D node) {
		if (node is Player) {
			_nakkiToTrigger?.PlayerEnteredTrigger();
		}
	}
}
