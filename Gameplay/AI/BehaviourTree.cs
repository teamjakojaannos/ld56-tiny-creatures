using Godot;

using Jakojaannos.CodeGen;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.AI;

[Tool]
[GlobalClass]
public partial class BehaviourTree : Node {
	[Export]
	[MustSetInEditor]
	public partial BTNode Root { get; set; }

	[Export]
	[MustSetInEditor]
	public AIState State {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_state);
		set => this.SetExportProperty(ref _state, value);
	}
	private AIState? _state;

	public override string[] _GetConfigurationWarnings() {
		return this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings());
	}

	public override void _PhysicsProcess(double delta) {
		if (Engine.IsEditorHint() || this.IsMissingRequiredProperty()) {
			return;
		}

		Root.Tick(State, (float)delta);
	}
}