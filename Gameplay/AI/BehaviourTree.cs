using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

[Tool]
public partial class BehaviourTree : Node {
	[Export]
	[MustSetInEditor]
	public BTNode Root {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_root);
		set => this.SetExportProperty(ref _root, value);
	}
	private BTNode? _root;

	public override void _PhysicsProcess(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		Root.Tick(state, (float)delta);
	}
}