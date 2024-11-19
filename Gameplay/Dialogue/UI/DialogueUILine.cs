using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.UI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class DialogueUILine : HBoxContainer {
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

	// FIXME: wrap portrait with a script and manage flipH and texture through that
	[Export]
	[MustSetInEditor]
	public TextureRect Portrait {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_portrait);
		set => this.SetExportProperty(ref _portrait, value);
	}
	private TextureRect? _portrait;

	[Export]
	[MustSetInEditor]
	public Control PortraitContainer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_portraitContainer);
		set => this.SetExportProperty(ref _portraitContainer, value);
	}
	private Control? _portraitContainer;

	[Export]
	[MustSetInEditor]
	public PositionAnimator PositionAnimator {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_positionAnimator);
		set => this.SetExportProperty(ref _positionAnimator, value);
	}
	private PositionAnimator? _positionAnimator;

	[Export]
	[MustSetInEditor]
	public HBoxContainer Layout {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_layout);
		set => this.SetExportProperty(ref _layout, value);
	}
	private HBoxContainer? _layout;

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
		_portraitContainer ??= GetNodeOrNull<TextureRect>("CharacterPortrait");
		_portrait ??= GetNodeOrNull<TextureRect>("CharacterPortrait/PortraitFrame/Portrait");
		_positionAnimator ??= GetChildren().OfType<PositionAnimator>().FirstOrDefault();
		_layout ??= this;

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
				DialogueSide.Left => PositionAnimator.Direction.Left,
				DialogueSide.Right => PositionAnimator.Direction.Right,
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
				DialogueSide.Left => PositionAnimator.Direction.Left,
				DialogueSide.Right => PositionAnimator.Direction.Right,
			};
		}

		if (_portrait is not null && _layout is not null) {
			var portraitChildIndex = Side switch {
				DialogueSide.Left => 0,
				DialogueSide.Right => -1,
			};
			MoveChild(PortraitContainer, portraitChildIndex);
			Layout.Alignment = Side switch {
				DialogueSide.Left => AlignmentMode.Begin,
				DialogueSide.Right => AlignmentMode.End,
			};
		}
	}

	public void OnAdded() {
		IsFullyVisible = false;
		Visible = false;
		Animation.Play("Added");
	}
}
