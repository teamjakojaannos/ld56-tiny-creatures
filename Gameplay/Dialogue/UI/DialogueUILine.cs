using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUILine : Control {
	[Export]
	public DialogueSide Side { get; set; } = DialogueSide.Left;

	[Export]
	[ExportGroup("Animation")]
	public float PercentageOffScreen {
		get => _percentageOffScreen;
		set {
			_percentageOffScreen = value;

			var totalLineWidth = GetRect().Size.X;
			var portraitWidth = _portrait?.GetRect().Size.X ?? 1.0f;
			var remappedValue = value * (portraitWidth / totalLineWidth);

			AnchorLeft = Side switch {
				DialogueSide.Right => remappedValue,
				DialogueSide.Left => -remappedValue,
			};
			AnchorRight = AnchorLeft + 1.0f;
		}
	}
	private float _percentageOffScreen;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public AnimationPlayer Animation {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animation);
		set => this.SetExportProperty(ref _animation, value);
	}
	private AnimationPlayer? _animation;

	[Export]
	[MustSetInEditor]
	public Control Portrait {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_portrait);
		set => this.SetExportProperty(ref _portrait, value);
	}
	private Control? _portrait;

	public uint LinePosition {
		get => _linePosition;
		internal set {
			_linePosition = value;
			Refresh();
		}
	}
	private uint _linePosition = 0;

	public bool IsFullyVisible { get; private set; } = false;

	public override void _Ready() {
		_animation ??= GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
		_portrait ??= GetNodeOrNull<Control>("CharacterPortrait");

		if (_animation is not null) {
			Animation.AnimationFinished += (animation) => {
				if (animation == "Added") {
					// TODO: start text scroll
					IsFullyVisible = true;
				}
			};
		}
	}

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

	public void OnAdded() {
		IsFullyVisible = false;
		Visible = false;
		Animation.Play("Added");
	}
}
