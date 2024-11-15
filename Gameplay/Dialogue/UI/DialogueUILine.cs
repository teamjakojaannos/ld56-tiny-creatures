using System;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUILine : Control {
	public uint LinePosition {
		get => _linePosition;
		internal set {
			_linePosition = value;
			Refresh();
		}
	}
	private uint _linePosition = 0;

	public bool IsFullyVisible => true;

	public void SkipEntryAnimation() {
		throw new NotImplementedException();
	}

	private void Refresh() {
		Modulate = new(
			Modulate.R,
			Modulate.G,
			Modulate.B,
			Mathf.Clamp(1.0f - LinePosition * 0.4f, 0.0f, 1.0f)
		);
	}
}
