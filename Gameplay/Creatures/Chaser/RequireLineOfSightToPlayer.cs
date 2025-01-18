
using System.Linq;

using Godot;

using Jakojaannos.CodeGen;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
[GlobalClass]
public partial class RequireLineOfSightToPlayer : BTNode {
	[Export]
	[MustSetInEditor]
	public RayCast2D LineOfSightRay {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_lineOfSightRay);
		set => this.SetExportProperty(ref _lineOfSightRay, value);
	}
	private RayCast2D? _lineOfSightRay;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override StatusCode Tick(AIState state, float delta) {
		var player = this.Persistent().Player;
		var targetPosition = player.GlobalPosition;

		// raycast wants target as relative to itself, not global
		var target = targetPosition - LineOfSightRay.GlobalPosition;
		LineOfSightRay.TargetPosition = target;
		LineOfSightRay.ForceRaycastUpdate();

		if (!LineOfSightRay.IsColliding()) {
			return StatusCode.Failure;
		}

		var collider = LineOfSightRay.GetCollider();
		return collider is Player
			? StatusCode.Success
			: StatusCode.Failure;
	}
}
