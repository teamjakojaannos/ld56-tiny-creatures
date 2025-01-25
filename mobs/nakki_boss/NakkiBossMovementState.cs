using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiBossMovementState : NakkiAiState {
	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiAiState NextState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nextState);
		set => this.SetExportProperty(ref _nextState, value);
	}
	private NakkiAiState? _nextState;

	[Export]
	[MustSetInEditor]
	public Node2D MovementTarget {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_movementTarget);
		set => this.SetExportProperty(ref _movementTarget, value);
	}
	private Node2D? _movementTarget;


	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void AiUpdate(NakkiV2 nakki) {
		if (nakki.HasReachedTarget()) {
			nakki.CurrentState = NextState;
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var relative = MovementTarget.GlobalPosition - nakki.GlobalPosition;
		nakki.SetProgressTarget(relative.X);
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }
}