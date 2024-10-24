using System;
using System.Collections.Generic;
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
        get => this.GetNotNullExportPropertyWithNullableBackingField(_otherScene);
        set => this.SetExportProperty(ref _otherScene, value, notifyPropertyListChanged: true);
    }
    private string? _otherScene;

    [Export]
    private NodePath? entranceNodePath;

    public bool IsOtherScenePreviewVisible {
        get => _previewScene is not null && _previewScene.IsInsideTree() && !_previewScene.IsQueuedForDeletion();
    }

    private void UpdatePreviewPosition() {
        if (_previewScene is null) {
            return;
        }

        var anchorNode = entranceNodePath is not null
                                ? _previewScene.GetNodeOrNull<Node2D>(entranceNodePath)
                                : null;
        var anchorOffset = anchorNode?.Position ?? Vector2.Zero;

        _previewScene.GlobalPosition = GlobalPosition - anchorOffset;
    }

    private Level? _previewScene;

    public override string[] _GetConfigurationWarnings() {
        return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
            .Union(this.CheckCommonConfigurationWarnings())
            .ToArray();
    }

	public override void _ValidateProperty(Godot.Collections.Dictionary property) {
        base._ValidateProperty(property);

         if (property["name"].AsStringName() == PropertyName.entranceNodePath) {
            var usage = PropertyUsageFlags.Storage | PropertyUsageFlags.NoEditor;
            property["usage"] = (int)usage;
        }
	}

	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList() {
        var properties = base._GetPropertyList() ?? new Godot.Collections.Array<Godot.Collections.Dictionary>();

        // If the other scene is specified, obtain a list of valid entrance
        // nodes and show a selection dropdown.
        if (_otherScene is not null) {
            var entranceNodeNames = FindOtherSceneEntrances()
                .Select(node => node.Item2);

            var hintString =
                !entranceNodeNames.Any()
                    ? "<none available>"
                    : string.Join(",", entranceNodeNames);
            properties.Add(new Godot.Collections.Dictionary() {
                { "name", "EntranceNode" },
                { "type", (int)Variant.Type.Int },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", hintString },
                { "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
            });

            properties.Add(new Godot.Collections.Dictionary() {
                { "name", "OtherScenePreview" },
                { "type", (int)Variant.Type.Bool },
                { "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
            });
        }

        return properties;
    }

    public override Variant _Get(StringName property) {
        string propertyName = property.ToString();
        if (propertyName == "EntranceNode") {
            if (entranceNodePath is not null) {
                var entranceNodes = FindOtherSceneEntrances();

                for (int optionIndex = 0; optionIndex < entranceNodes.Count; optionIndex++) {
                    var (path, _) = entranceNodes[optionIndex];
                    if (path == entranceNodePath) {
                        return optionIndex;
                    }
                }
            }

            entranceNodePath = null;
            NotifyPropertyListChanged();
            return 0;
        } else if (propertyName == "OtherScenePreview") {
            return IsOtherScenePreviewVisible;
        }

        return default;
    }

    public override bool _Set(StringName property, Variant value) {
        string propertyName = property.ToString();
        if (propertyName == "EntranceNode") {
            var optionIndex = value.AsInt32();
            TrySelectEntranceNode(optionIndex);
            UpdatePreviewPosition();

            return true;
        } else if (propertyName == "OtherScenePreview" && Engine.IsEditorHint()) {
            var valueBool = value.AsBool();
            if (valueBool && _otherScene is null) {
                GD.PrintErr("Cannot preview a scene: other scene is not configured!");
                return true;
            }

            if (_previewScene is not null) {
                _previewScene.GetParent()?.RemoveChild(_previewScene);
                _previewScene.QueueFree();
                _previewScene = null;
            }

            if (valueBool) {
                var resource = ResourceLoader.Load<PackedScene>(_otherScene);
                _previewScene = resource.Instantiate<Level>();
                _previewScene.Name = "Other scene preview";

                if (entranceNodePath is null) {
                    TrySelectEntranceNode(0);
                }

                UpdatePreviewPosition();
                AddChild(_previewScene);
            }
        }
        return false;
    }

    private void TrySelectEntranceNode(int optionIndex) {
        var entranceNodes = FindOtherSceneEntrances();
        if (!entranceNodes.Any()) {
            entranceNodePath = null;
            UpdatePreviewPosition();
            return;
        }

        var (path, _) = entranceNodes[Mathf.Min(optionIndex, entranceNodes.Count - 1)];
        entranceNodePath = path;
    }

    private List<(NodePath, StringName)> FindOtherSceneEntrances() {
        // Other scene is not properly set yet => return empty list
        if (_otherScene is null || _otherScene.Trim() == "" || _otherScene.Trim() == "res://") {
            return new();
        }

        var entranceNodes = new List<(NodePath, StringName)>();

        var resource = ResourceLoader.Load<PackedScene>(OtherScene);
        var sceneState = resource.GetState();
        var nodesInScene = sceneState.GetNodeCount();
        for (int nodeIndex = 0; nodeIndex < nodesInScene; nodeIndex++) {
            var isEntranceNode = sceneState.GetNodeGroups(nodeIndex).Contains(Groups.Name.LEVEL_ENTRANCE);
            if (!isEntranceNode) {
                continue;
            }

            entranceNodes.Add((
                sceneState.GetNodePath(nodeIndex),
                sceneState.GetNodeName(nodeIndex)
            ));
        }

        return entranceNodes;
    }

    public Vector2 ExitDirection => Transform.BasisXform(Vector2.Right);

    public override void _ExitTree() {
        base._ExitTree();
        _previewScene?.QueueFree();
        _previewScene = null;
    }

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
