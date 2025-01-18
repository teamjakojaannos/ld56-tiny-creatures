using System;
using System.Linq;

using Godot;

using Jakojaannos.CodeGen;

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
	[MustSetInEditor]
	public Label TextElement {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_textElement);
		set => this.SetExportProperty(ref _textElement, value);
	}
	private Label? _textElement;

	[Export]
	[MustSetInEditor]
	public Timer TextScrollTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_textScrollTimer);
		set => this.SetExportProperty(ref _textScrollTimer, value);
	}
	private Timer? _textScrollTimer;

	[Export]
	[MustSetInEditor]
	public AudioStreamPlayer SpeakingSfx {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_speakingSfx);
		set => this.SetExportProperty(ref _speakingSfx, value);
	}
	private AudioStreamPlayer? _speakingSfx;

	public bool IsAllTextVisible => TextElement.VisibleCharacters == Text.Length;

	public override void _Ready() {
		base._Ready();

		_textScrollTimer ??= GetChildren().OfType<Timer>().FirstOrDefault();
		_speakingSfx ??= GetChildren().OfType<AudioStreamPlayer>().FirstOrDefault();
		_textElement ??= GetChildren().OfType<Label>().FirstOrDefault();

		if (_textScrollTimer is null || _speakingSfx is null || _textElement is null) {
			return;
		}

		TextScrollTimer.Timeout += () => {
			if (IsAllTextVisible) {
				TextScrollTimer.Stop();
				SpeakingSfx.Stop();
				IsFullyVisible = true;
			}

			TextElement.VisibleCharacters++;
		};
	}

	protected override void OnAddedFinished() {
		TextScrollTimer.Start();
		SpeakingSfx.Play();
	}

	public override void SkipEntryAnimation() {
		base.SkipEntryAnimation();

		TextScrollTimer.Stop();
		SpeakingSfx.Stop();
		TextElement.VisibleCharacters = -1;
	}

	public override void OnAdded() {
		base.OnAdded();

		TextElement.VisibleCharacters = 0;
		TextElement.CustomMinimumSize = TextElement.Size;
	}
}
