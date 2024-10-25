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
	private NodePath entranceNodePath = "";

	public bool IsOtherScenePreviewVisible {
		get => _previewScene is not null && _previewScene.IsInsideTree() && !_previewScene.IsQueuedForDeletion();
	}

	private void UpdatePreviewPosition() {
		if (_previewScene is null) {
			return;
		}

		Levels.AdjustLevelPositionRelativeToCurrent(_previewScene, entranceNodePath, this);
	}

	private Level? _previewScene;
	private StringName? previewSceneOriginalName;
	private Vector2 previewSceneOriginalPosition = Vector2.Zero;

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

	private void OpenOtherScene() {
		if (_otherScene is not null && Engine.IsEditorHint()) {
			EditorInterface.Singleton.CallDeferred(EditorInterface.MethodName.OpenSceneFromPath, OtherScene);
		}
	}

	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList() {
		var properties = base._GetPropertyList() ?? new Godot.Collections.Array<Godot.Collections.Dictionary>();

		// If the other scene is specified, obtain a list of valid entrance
		// nodes and show a selection dropdown.
		if (_otherScene is not null) {
			var editIconName = "Edit";
			properties.Add(new Godot.Collections.Dictionary() {
				{ "name", "OpenOtherScene" },
				{ "type", (int)Variant.Type.Callable },
				{ "hint", (int)PropertyHint.ToolButton },
				{ "hint_string", $"{MethodName.OpenOtherScene},{editIconName}" },
				{ "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
			});

			var entranceNodeNames = FindOtherSceneEntrances()
				.Select(node => node.Item2);

			bool isEntranceAvailable = entranceNodeNames.Any();

			var hintString =
				isEntranceAvailable
					? string.Join(",", entranceNodeNames)
					: "<none available>";
			properties.Add(new Godot.Collections.Dictionary() {
				{ "name", "EntranceNode" },
				{ "type", (int)Variant.Type.Int },
				{ "hint", (int)PropertyHint.Enum },
				{ "hint_string", hintString },
				{ "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
			});

			if (isEntranceAvailable) {
				// Don't allow modifying the offset without seeing the results
				var usage =
					IsOtherScenePreviewVisible
						? PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState
						: PropertyUsageFlags.Editor | PropertyUsageFlags.ReadOnly | PropertyUsageFlags.NoInstanceState;
				properties.Add(new Godot.Collections.Dictionary() {
					{ "name", "EntranceNodeOffset" },
					{ "type", (int)Variant.Type.Vector2 },
					{ "usage", (int)usage },
				});
			}

			properties.Add(new Godot.Collections.Dictionary() {
				{ "name", "OtherScenePreview" },
				{ "type", (int)Variant.Type.Bool },
				{ "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
			});
		}

		return properties;
	}

	public override Variant _Get(StringName property) {
		if (property == "EntranceNode") {
			if (!entranceNodePath.IsEmpty) {
				var entranceNodes = FindOtherSceneEntrances();

				for (int optionIndex = 0; optionIndex < entranceNodes.Count; optionIndex++) {
					var (path, _, _) = entranceNodes[optionIndex];
					if (path == entranceNodePath) {
						return optionIndex;
					}
				}
			}

			entranceNodePath = "";
			NotifyPropertyListChanged();
			return 0;
		} else if (property == "OtherScenePreview") {
			return IsOtherScenePreviewVisible;
		} else if (property == "EntranceNodeOffset") {
			if (!entranceNodePath.IsEmpty) {
				var entranceNodes = FindOtherSceneEntrances();

				for (int optionIndex = 0; optionIndex < entranceNodes.Count; optionIndex++) {
					var (path, _, position) = entranceNodes[optionIndex];
					if (path == entranceNodePath) {
						return position;
					}
				}
			}

			entranceNodePath = "";
			NotifyPropertyListChanged();
			return Vector2.Zero;
		} else if (property == "OpenOtherScene") {
			return Callable.From(OpenOtherScene);
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
				previewSceneOriginalName = _previewScene.Name;
				previewSceneOriginalPosition = _previewScene.Position;
				_previewScene.Name = "Other scene preview";
				_previewScene.ProcessMode = ProcessModeEnum.Disabled;

				if (entranceNodePath.IsEmpty) {
					TrySelectEntranceNode(0);
				}

				UpdatePreviewPosition();
				AddChild(_previewScene);
			}
			NotifyPropertyListChanged();
		} else if (propertyName == "EntranceNodeOffset" && Engine.IsEditorHint()) {
			if (_previewScene is not null && _otherScene is not null) {
				var entranceMarker = _previewScene.GetNode<Node2D>(entranceNodePath);
				entranceMarker.Position = value.AsVector2();

				var sceneToSave = new PackedScene();
				_previewScene.Position = previewSceneOriginalPosition;
				if (previewSceneOriginalName is not null) {
					_previewScene.Name = previewSceneOriginalName;
				}
				// FIXME: store the original
				_previewScene.ProcessMode = ProcessModeEnum.Inherit;

				// FIXME: debounce the save a bit
				sceneToSave.Pack(_previewScene);
				ResourceSaver.Save(sceneToSave, OtherScene);
				sceneToSave.TakeOverPath(OtherScene);
				CallDeferred(MethodName.ReloadPreviewScene);

				// Restore the preview
				_previewScene.Name = "Other scene preview";
				_previewScene.ProcessMode = ProcessModeEnum.Disabled;

				if (entranceNodePath.IsEmpty) {
					TrySelectEntranceNode(0);
				}

				UpdatePreviewPosition();
				NotifyPropertyListChanged();
			}
		}
		return false;
	}

	private void ReloadPreviewScene() {
		EditorInterface.Singleton.ReloadSceneFromPath(OtherScene);
	}

	private void TrySelectEntranceNode(int optionIndex) {
		var entranceNodes = FindOtherSceneEntrances();
		if (!entranceNodes.Any()) {
			entranceNodePath = "";
			UpdatePreviewPosition();
			return;
		}

		var (path, _, _) = entranceNodes[Mathf.Min(optionIndex, entranceNodes.Count - 1)];
		entranceNodePath = path;
	}

	private List<(NodePath, StringName, Vector2)> FindOtherSceneEntrances() {
		// Other scene is not properly set yet => return empty list
		if (_otherScene is null || _otherScene.Trim() == "" || _otherScene.Trim() == "res://") {
			return new();
		}

		var entranceNodes = new List<(NodePath, StringName, Vector2)>();

		// FIXME: cache the loaded scene to a local field(?)
		var resource = ResourceLoader.Load<PackedScene>(OtherScene);
		var sceneState = resource.GetState();
		var nodesInScene = sceneState.GetNodeCount();
		for (int nodeIndex = 0; nodeIndex < nodesInScene; nodeIndex++) {
			var isEntranceNode = sceneState.GetNodeGroups(nodeIndex).Contains(Groups.Name.LEVEL_ENTRANCE);
			if (!isEntranceNode) {
				continue;
			}

			var position = Vector2.Zero;
			var nodePropertyCount = sceneState.GetNodePropertyCount(nodeIndex);
			for (int propertyIndex = 0; propertyIndex < nodePropertyCount; propertyIndex++) {
				var propertyName = sceneState.GetNodePropertyName(nodeIndex, propertyIndex);
				if (propertyName == "position") {
					position = sceneState.GetNodePropertyValue(nodeIndex, propertyIndex).AsVector2();
					break;
				}
			}

			entranceNodes.Add((
				sceneState.GetNodePath(nodeIndex),
				sceneState.GetNodeName(nodeIndex),
				position
			));
		}

		return entranceNodes;
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
			this.Levels().CallDeferred(Levels.MethodName.TransitionToLevel, OtherScene, entranceNodePath, this);
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
