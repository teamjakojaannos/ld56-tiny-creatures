using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.UI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUILine : Control {
	[Export]
	public DialogueSide Side {
		get => _side;
		set {
			_side = value;
			Refresh();
		}
	}
	private DialogueSide _side = DialogueSide.Left;

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

	[Export]
	[MustSetInEditor]
	public PositionAnimator PositionAnimator {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_positionAnimator);
		set => this.SetExportProperty(ref _positionAnimator, value);
	}
	private PositionAnimator? _positionAnimator;

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
		_positionAnimator ??= GetChildren().OfType<PositionAnimator>().FirstOrDefault();

		if (_animation is not null) {
			Animation.AnimationFinished += (animation) => {
				if (animation == "Added") {
					// TODO: start text scroll
					IsFullyVisible = true;
				}
			};
		}

		if (_positionAnimator is not null) {
			PositionAnimator.ScrollDirection = Side switch {
				DialogueSide.Left => PositionAnimator.Direction.Right,
				DialogueSide.Right => PositionAnimator.Direction.Left,
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

		if (_positionAnimator is not null) {
			PositionAnimator.ScrollDirection = Side switch {
				DialogueSide.Left => PositionAnimator.Direction.Right,
				DialogueSide.Right => PositionAnimator.Direction.Left,
			};
		}
	}

	public void OnAdded() {
		IsFullyVisible = false;
		Visible = false;
		Animation.Play("Added");
	}
}
