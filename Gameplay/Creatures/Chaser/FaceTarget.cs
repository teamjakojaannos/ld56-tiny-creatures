using System.Linq;

using Godot;

using Jakojaannos.CodeGen;

using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
[GlobalClass]
public partial class FaceTarget : BTNode {
	[Export]
	public float TurnSpeedDegrees { get; set; } = 45.0f;

	[Export(PropertyHint.Range, "0,360")]
	public float ConeAngleOffsetDegrees { get; set; } = 90.0f;

	/// <summary>
	/// Allow the Behaviour Tree to process more nodes in sequence after this
	/// node has sucessfully executed. That is, minor adjustments (adjustments
	/// which finish within a single tick) do not cause the node to report
	/// status as `Running`.
	/// </summary>
	[Export]
	public bool SucceedImmediately { get; set; } = false;

	[Export]
	[MustSetInEditor]
	public Node2D Actor {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_actor);
		set => this.SetExportProperty(ref _actor, value);
	}
	private Node2D? _actor;

	[Export]
	[MustSetInEditor]
	public Node2D SightRoot {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sightRoot);
		set => this.SetExportProperty(ref _sightRoot, value);
	}
	private Node2D? _sightRoot;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override StatusCode Tick(AIState state, float delta) {
		var target = state.GetState("target")?.AsVector2();
		if (target is not Vector2 targetPoint) {
			return StatusCode.Failure;
		}

		var coneAngleOffsetRadians = Mathf.DegToRad(ConeAngleOffsetDegrees);
		var desiredAngle = Actor.GlobalPosition.AngleToPoint(targetPoint) + coneAngleOffsetRadians;

		var remaining = Mathf.Abs(Mathf.AngleDifference(desiredAngle, SightRoot.Rotation));
		if (remaining < 0.1f) {
			SightRoot.Rotation = desiredAngle;
			return StatusCode.Success;
		}

		var degreesPerSecond = TurnSpeedDegrees * delta;
		var radiansPerSecond = Mathf.DegToRad(degreesPerSecond);

		var newRotation = Mathf.RotateToward(SightRoot.Rotation, desiredAngle, radiansPerSecond);
		SightRoot.Rotation = newRotation;

		var remainingAfterRotation = Mathf.Abs(Mathf.AngleDifference(desiredAngle, SightRoot.Rotation));
		return remainingAfterRotation < 0.1f && SucceedImmediately
			? StatusCode.Success
			: StatusCode.Running;
	}
}
