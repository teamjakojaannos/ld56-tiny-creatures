using Godot;

using Jakojaannos.WisperingWoods.Characters.Wisp;
using Jakojaannos.WisperingWoods.Util.Editor;


namespace Jakojaannos.WisperingWoods.Characters.Player;

[Tool]
[GlobalClass]
public partial class WispTargetPosition : StaticBody2D {
	[Export]
	public float FollowDistance { get; set; } = 75.0f;

	[Export]
	[MustSetInEditor]
	[ExportCategory("Prewire")]
	public PlayerCharacter Player {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_player);
		set => this.SetExportProperty(ref _player, value);
	}
	private PlayerCharacter? _player;

	[Export]
	[MustSetInEditor]
	public WispCharacter Wisp {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_wisp);
		set => this.SetExportProperty(ref _wisp, value);
	}
	private WispCharacter? _wisp;

	[Export]
	[MustSetInEditor]
	public CollisionShape2D DebugShape {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_debugShape);
		set => this.SetExportProperty(ref _debugShape, value);
	}
	private CollisionShape2D? _debugShape;

	public float DebugRadius {
		get => (DebugShape.Shape as CircleShape2D)?.Radius ?? 0.0f;
		set {
			if (DebugShape.Shape is CircleShape2D circle) {
				circle.Radius = value;
			}
		}
	}

	private Vector2 _targetPosition = Vector2.Zero;

	public override string[] _GetConfigurationWarnings() {
		return [.. this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings())];
	}

	public void ResetIdlePosition() {
		_targetPosition = Player.GlobalPosition + Vector2.Right * FollowDistance;
	}

	public override void _PhysicsProcess(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		MoveWispTargetPosition((float)delta);
	}

	private void MoveWispTargetPosition(float delta) {
		var inputDirection = Player.InputDirection;

		if (Wisp.InteractTargetPosition is Vector2 targetPos) {
			_targetPosition = targetPos;
		} else if (!inputDirection.IsZeroApprox()) {
			_targetPosition = Player.GlobalPosition + inputDirection * FollowDistance;
		}

		var distance = GlobalPosition.DistanceSquaredTo(_targetPosition);
		GlobalPosition = GlobalPosition.Lerp(_targetPosition, 10.0f * delta);
	}
}
