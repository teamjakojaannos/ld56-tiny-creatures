using Godot;

using Jakojaannos.CodeGen;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.UI;

[Tool]
[GlobalClass]
public partial class PositionAnimator : Node {
	public enum Direction {
		Up,
		Down,
		Left,
		Right
	}

	[Export]
	[MustSetInEditor]
	public Control AnimatedControl {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animatedControl);
		set => this.SetExportProperty(ref _animatedControl, value);
	}
	private Control? _animatedControl;

	[Export]
	public Control? UseSizeOf { get; set; }

	[Export]
	public float Offset { get; set; } = 0.0f;

	[Export]
	public Direction ScrollDirection { get; set; } = Direction.Right;

	[Export]
	public float PercentageOffScreen {
		get => _percentageOffScreen;
		set {
			_percentageOffScreen = value;
			var scrollValue = CalculateScrollValue(_percentageOffScreen);

			// FIXME: there are floating point precision errors here (e.g. bottom margin may end up 0.99998 instead of 1.0 when top margin is 1.0)
			if (ScrollDirection == Direction.Left || ScrollDirection == Direction.Right) {
				var difference = AnimatedControl.AnchorRight - AnimatedControl.AnchorLeft;
				AnimatedControl.AnchorLeft = scrollValue;
				AnimatedControl.AnchorRight = scrollValue + difference;
			} else {
				var difference = AnimatedControl.AnchorBottom - AnimatedControl.AnchorTop;
				AnimatedControl.AnchorTop = scrollValue;
				AnimatedControl.AnchorBottom = scrollValue + difference;
			}
		}
	}
	private float _percentageOffScreen;

	private float CalculateScrollValue(float value) {
		var totalSize = ScrollDirection switch {
			Direction.Left or Direction.Right => AnimatedControl.GetRect().Size.X,
			Direction.Up or Direction.Down => AnimatedControl.GetRect().Size.Y,
		};

		var targetSize = ScrollDirection switch {
			Direction.Left or Direction.Right => UseSizeOf?.GetRect().Size.X,
			Direction.Up or Direction.Down => UseSizeOf?.GetRect().Size.Y,
		};
		var sizeFactor = targetSize is not null
			? (float)(targetSize / totalSize)
			: 1.0f;

		var remappedValue = value * sizeFactor;

		return Offset + ScrollDirection switch {
			Direction.Left or Direction.Up => -remappedValue,
			Direction.Right or Direction.Down => remappedValue,
		};
	}

	public override void _Ready() {
		_animatedControl ??= GetParentOrNull<Control>();
	}
}
