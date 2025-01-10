using System;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUIChoiceLineOption : Control {
	[Export]
	public Color DefaultColor { get; set; } = new(Colors.White, 0.75f);
	public Color LockedColor { get; set; } = new(Colors.White, 0.33f);
	public Color HighlightedColor { get; set; } = Colors.White;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Label OrdinalElement {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_ordinalElement);
		set => this.SetExportProperty(ref _ordinalElement, value);
	}
	private Label? _ordinalElement;

	[Export]
	[MustSetInEditor]
	public Label TextElement {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_textElement);
		set => this.SetExportProperty(ref _textElement, value);
	}
	private Label? _textElement;

	public bool Highlighted {
		get => _highlighted;
		internal set {
			_highlighted = value;
			RefreshVisuals();
		}
	}
	private bool _highlighted = false;

	public bool Locked {
		get => _locked;
		internal set {
			_locked = value;
			RefreshVisuals();
		}
	}

	private void RefreshVisuals() {
		if (Highlighted) {
			Modulate = HighlightedColor;
		} else if (Locked) {
			Modulate = LockedColor;
		} else {
			Modulate = DefaultColor;
		}
	}

	private bool _locked = false;

	internal void Setup(uint optionIndex, DialogueChoiceLine.Option option) {
		var ordinal = optionIndex + 1;
		OrdinalElement.Text = $"{ordinal}.";
		TextElement.Text = option.Text;
		Highlighted = optionIndex == 0;
		Locked = false;
	}
}
