using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.World;

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

	// Exported, but hidden in editor.
	[Export]
	private NodePath _entranceNodePath = "";
	internal NodePath EntranceNodePath => _entranceNodePath;

	[Export(PropertyHint.Range, "0,360,5,radians_as_degrees")]
	public float ExitDirection {
		get => _exitDirection;
		set => this.SetExportProperty(ref _exitDirection, value, requestRedraw: true);
	}
	private float _exitDirection = 0.0f;

	public Vector2 ExitDirectionVec => Vector2.FromAngle(ExitDirection);

	public bool IsOtherScenePreviewVisible {
		get => _previewScene is not null && _previewScene.IsInsideTree() && !_previewScene.IsQueuedForDeletion();
	}

	private void UpdatePreviewPosition() {
		if (_previewScene is null) {
			return;
		}

		Levels.AdjustLevelPositionRelativeToCurrent(_previewScene, _entranceNodePath, this);
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

		if (property["name"].AsStringName() == PropertyName._entranceNodePath) {
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
				{ "hint", (int)PropertyHint.None },
				//{ "hint", (int)PropertyHint.ToolButton },
				{ "hint_string", $"{MethodName.OpenOtherScene},{editIconName}" },
				{ "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState) },
			});

			var entranceNodeNames = FindOtherSceneEntrances()
				.Select(node => node.Name);

			var isEntranceAvailable = entranceNodeNames.Any();

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
			if (!_entranceNodePath.IsEmpty) {
				var entranceNodes = FindOtherSceneEntrances();

				for (var optionIndex = 0; optionIndex < entranceNodes.Count; optionIndex++) {
					var entrance = entranceNodes[optionIndex];
					if (entrance.Path == _entranceNodePath) {
						return optionIndex;
					}
				}
			}

			_entranceNodePath = "";
			NotifyPropertyListChanged();
			return 0;
		} else if (property == "OtherScenePreview") {
			return IsOtherScenePreviewVisible;
		} else if (property == "EntranceNodeOffset") {
			if (!_entranceNodePath.IsEmpty) {
				var entranceNodes = FindOtherSceneEntrances();

				for (var optionIndex = 0; optionIndex < entranceNodes.Count; optionIndex++) {
					var entrance = entranceNodes[optionIndex];
					if (entrance.Path == _entranceNodePath) {
						return entrance.Position;
					}
				}
			}

			_entranceNodePath = "";
			NotifyPropertyListChanged();
			return Vector2.Zero;
		} else if (property == "OpenOtherScene") {
			return Callable.From(OpenOtherScene);
		}

		return default;
	}

	public override bool _Set(StringName property, Variant value) {
		if (property == "EntranceNode") {
			var optionIndex = value.AsInt32();
			TrySelectEntranceNode(optionIndex);
			UpdatePreviewPosition();

			return true;
		} else if (property == "OtherScenePreview" && Engine.IsEditorHint()) {
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

				if (_entranceNodePath.IsEmpty) {
					GD.PrintErr("Entrance node path is empty");
					TrySelectEntranceNode(0);
				}

				AddChild(_previewScene);
				UpdatePreviewPosition();
			}
			NotifyPropertyListChanged();
		} else if (property == "EntranceNodeOffset" && Engine.IsEditorHint()) {
			if (_previewScene is not null && _otherScene is not null) {
				var entranceMarker = _previewScene.GetNode<Node2D>(_entranceNodePath);
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

				if (_entranceNodePath.IsEmpty) {
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
			_entranceNodePath = "";
			UpdatePreviewPosition();
			return;
		}

		var entrance = entranceNodes[Mathf.Min(optionIndex, entranceNodes.Count - 1)];
		_entranceNodePath = entrance.Path;
	}

	private List<EntranceNode> FindOtherSceneEntrances() {
		// Other scene is not properly set yet => return empty list
		if (_otherScene is null || _otherScene.Trim() == "" || _otherScene.Trim() == "res://") {
			return new();
		}

		var entranceNodes = new List<EntranceNode>();

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

			entranceNodes.Add(new EntranceNode() {
				Path = sceneState.GetNodePath(nodeIndex),
				Name = sceneState.GetNodeName(nodeIndex),
				Position = position
			});
		}

		return entranceNodes;
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

		var isOnExitSideRatio = ExitDirectionVec.Dot(directionToPlayer);
		var isExiting = isOnExitSideRatio > 0.0f;

		if (isExiting) {
			GD.Print("Transitioning!");
			this.Levels().CallDeferred(Levels.MethodName.TransitionToLevel, OtherScene, _entranceNodePath, this);
		}
	}

	public override void _Draw() {
		base._Draw();

		if (Engine.IsEditorHint()) {
			DrawTransitionDirection();
		}
	}

	private void DrawTransitionDirection() {
		var from = Vector2.Zero;
		var color = Colors.Red;

		var to = from + ExitDirectionVec * DIRECTION_LINE_LENGTH;
		DrawLine(from, to, color, width: 2.0f, antialiased: false);

		var arrowheadPos = to - ExitDirectionVec * DIRECTION_LINE_ARROWHEAD_SIZE;
		DrawArc(arrowheadPos, DIRECTION_LINE_ARROWHEAD_SIZE, ExitDirection - Mathf.DegToRad(90.0f), ExitDirection + Mathf.DegToRad(90.0f), 3, color, width: 2.0f, antialiased: false);
	}

	public readonly struct EntranceNode {
		public readonly NodePath Path { init; get; }
		public readonly StringName Name { init; get; }
		public readonly Vector2 Position { init; get; }
	}
}
