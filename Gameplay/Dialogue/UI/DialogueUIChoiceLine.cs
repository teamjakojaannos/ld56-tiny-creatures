using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUIChoiceLine : DialogueUILine {
	public int HighlightedOption {
		get => _highlightedOption;
		set {
			_highlightedOption = value;
			UpdateHighlighted();
		}
	}
	private int _highlightedOption = 0;
	public int LastOptionIndex => OptionCount - 1;

	public IEnumerable<DialogueUIChoiceLineOption> Options => OptionsContainer
		.GetChildren()
		.OfType<DialogueUIChoiceLineOption>();
	public int OptionCount => Options.Count();

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Control OptionsContainer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_optionsContainer);
		set => this.SetExportProperty(ref _optionsContainer, value);
	}
	private Control? _optionsContainer;

	[Export]
	[MustSetInEditor]
	public PackedScene OptionTemplate {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_optionTemplate);
		set => this.SetExportProperty(ref _optionTemplate, value);
	}
	private PackedScene? _optionTemplate;

	public void LockSelection() {
		var optionIndex = 0;
		foreach (var option in Options) {
			optionIndex++;
			var isSelected = optionIndex == HighlightedOption;
			option.Highlighted = isSelected;
			option.Locked = !isSelected;
		}
	}

	public void UpdateHighlighted() {
		var optionIndex = 0;
		foreach (var option in Options) {
			optionIndex++;
			var isSelected = optionIndex == HighlightedOption;
			option.Highlighted = isSelected;
		}
	}

	internal void ClearOptions() {
		foreach (var option in Options) {
			option.QueueFree();
		}
	}

	internal void AddOption(DialogueUIChoiceLineOption option) {
		OptionsContainer.AddChild(option);
	}
}
