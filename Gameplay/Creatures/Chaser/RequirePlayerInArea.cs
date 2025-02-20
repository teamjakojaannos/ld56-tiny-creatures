
using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
[GlobalClass]
public partial class RequirePlayerInArea : BTNode {
	[Export]
	[MustSetInEditor]
	public Area2D Area {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_area);
		set => this.SetExportProperty(ref _area, value);
	}
	private Area2D? _area;

	private bool _isPlayerInTrigger = false;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint() || this.IsMissingRequiredProperty()) {
			return;
		}

		Area.BodyEntered += (body) => {
			if (body is PlayerCharacter player) {
				_isPlayerInTrigger = true;
			}
		};
		Area.BodyExited += (body) => {
			if (body is PlayerCharacter player) {
				_isPlayerInTrigger = false;
			}
		};
	}

	public override StatusCode Tick(AIState state, float delta) {
		return _isPlayerInTrigger
			? StatusCode.Success
			: StatusCode.Failure;
	}
}
