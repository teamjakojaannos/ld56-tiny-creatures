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

    [Export]
    public bool PreviewOtherScene {
        get => _previewOtherScene && _previewScene is not null && _previewScene.IsInsideTree() && !_previewScene.IsQueuedForDeletion();
        set {
            if (Engine.IsEditorHint()) {
                _previewOtherScene = value;

                if (_previewScene is not null) {
                    _previewScene.GetParent()?.RemoveChild(_previewScene);
                    _previewScene.QueueFree();
                    _previewScene = null;
                }

                if (_previewOtherScene && _otherScene is not null) {
                    var resource = ResourceLoader.Load<PackedScene>(_otherScene);
                    _previewScene = resource.Instantiate<Level>();
                    _previewScene.Name = "Other scene preview";
                    AddChild(_previewScene);
                }
            }
        }
    }

    private bool _previewOtherScene = false;
    private Level? _previewScene;

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
