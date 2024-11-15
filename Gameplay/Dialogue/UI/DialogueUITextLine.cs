using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUITextLine : DialogueUILine {
	[Export]
	public string Text {
		get => _textElement?.Text ?? "A quick brown fox jumps over a lazy dog.";
		set {
			if (_textElement is not null) {
				_textElement.Text = value;
			}
		}
	}

	[Export]
	[ExportGroup("Prewire")]
	public Label TextElement {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_textElement);
		set => this.SetExportProperty(ref _textElement, value);
	}
	private Label? _textElement;

	public override void _Ready() {
		_textElement ??= GetChildren().OfType<Label>().FirstOrDefault();
		base._Ready();
	}
}
