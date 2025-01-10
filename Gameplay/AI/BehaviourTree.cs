using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

[Tool]
[GlobalClass]
public partial class BehaviourTree : Node {
	[Export]
	[MustSetInEditor]
	public BTNode Root {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_root);
		set => this.SetExportProperty(ref _root, value);
	}
	private BTNode? _root;

	[Export]
	[MustSetInEditor]
	public AIState State {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_state);
		set => this.SetExportProperty(ref _state, value);
	}
	private AIState? _state;

	public override void _PhysicsProcess(double delta) {
		if (Engine.IsEditorHint() || this.IsMissingRequiredProperty()) {
			return;
		}

		Root.Tick(State, (float)delta);
	}
}