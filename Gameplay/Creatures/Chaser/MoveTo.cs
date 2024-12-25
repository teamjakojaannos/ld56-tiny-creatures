using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
[GlobalClass]
public partial class MoveTo : BTNode {
	[Export]
	public float Speed { get; set; } = 60.0f;

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
	public NavigationAgent2D NavigationAgent {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_navigationAgent);
		set => this.SetExportProperty(ref _navigationAgent, value);
	}
	private NavigationAgent2D? _navigationAgent;

	[Export]
	[MustSetInEditor]
	public CharacterBody2D Actor {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_actor);
		set => this.SetExportProperty(ref _actor, value);
	}
	private CharacterBody2D? _actor;

	private bool _isNavSetupDone = false;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		if (Engine.IsEditorHint() || this.IsMissingRequiredProperty()) {
			return;
		}

		CallDeferred(MethodName.WaitNavSetup);
	}

	private async void WaitNavSetup() {
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		_isNavSetupDone = true;
	}

	public override StatusCode Tick(AIState state, float delta) {
		if (!_isNavSetupDone) {
			return StatusCode.Failure;
		}

		var target = state.GetState("target")?.AsVector2();
		if (target is not Vector2 targetPoint) {
			return StatusCode.Failure;
		}

		NavigationAgent.TargetPosition = targetPoint;

		if (NavigationAgent.IsNavigationFinished()) {
			state.SetState("target", null);
			return StatusCode.Success;
		}

		var currentAgentPosition = Actor.GlobalPosition;
		var nextPathPosition = NavigationAgent.GetNextPathPosition();
		var velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;

		Actor.Velocity = velocity;
		Actor.MoveAndSlide();

		return NavigationAgent.IsNavigationFinished() && SucceedImmediately
			? StatusCode.Success
			: StatusCode.Running;
	}
}
