using System;
using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class LevelTransition : Area2D {
    public const float DIRECTION_LINE_LENGTH = 100.0f;
    public const float DIRECTION_LINE_ARROWHEAD_SIZE = 7.5f;

    [Export(PropertyHint.File, "*.tscn")]
    [MustSetInEditor]
    public string OtherScene {
        get => _otherScene ?? this.AssertNotNullOutsideEditor<string>();
        set {
            _otherScene = value;
            UpdateConfigurationWarnings();
        }
    }
    private string? _otherScene;

    public override string[] _GetConfigurationWarnings() {
        return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
            .Union(this.CheckCommonConfigurationWarnings())
            .ToArray();
    }

    public Vector2 ExitDirection => Transform.BasisXform(Vector2.Right);

    public override void _Ready() {
        base._Ready();
        if (Engine.IsEditorHint()) {
            return;
        }

        BodyExited += OnPlayerExitedTrigger;
    }

    private void OnPlayerExitedTrigger(Node2D body) {
        if (body is not Player player) {
            return;
        }

        var directionToPlayer = GlobalPosition.DirectionTo(player.GlobalPosition);

        var isOnExitSideRatio = ExitDirection.Dot(directionToPlayer);
        var isExiting = isOnExitSideRatio > 0.0f;

        if (isExiting) {
            GD.Print("Transitioning!");
            this.Levels().CallDeferred(Levels.MethodName.TransitionToLevel, OtherScene);
        }
    }

    public override void _Draw() {
        base._Draw();

        if (Engine.IsEditorHint()) {
            DrawTransitionDirection();
        }
    }

    private void DrawTransitionDirection() {
        var from = Position;
		var color = Colors.Red;

        var to = from + ExitDirection * DIRECTION_LINE_LENGTH;
        DrawLine(from, to, color, width: 2.0f, antialiased: false);

        var arrowheadPos = to - ExitDirection * DIRECTION_LINE_ARROWHEAD_SIZE;
        DrawArc(arrowheadPos, DIRECTION_LINE_ARROWHEAD_SIZE, Mathf.DegToRad(-90.0f), Mathf.DegToRad(90.0f), 3, color, width: 2.0f, antialiased: false);
    }
}
