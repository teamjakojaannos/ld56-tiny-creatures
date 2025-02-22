using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;


namespace Jakojaannos.WisperingWoods.Characters.Player;

[Tool]
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

	public override void _PhysicsProcess(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		MoveWispTargetPosition();
	}

	private void MoveWispTargetPosition() {
		var inputDirection = Player.InputDirection;

		if (inputDirection.LengthSquared() < 0.001f) {
			return;
		}

		GlobalPosition = Player.GlobalPosition + inputDirection * FollowDistance;
	}
}
