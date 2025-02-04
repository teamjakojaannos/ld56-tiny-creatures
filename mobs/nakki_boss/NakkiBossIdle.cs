using Godot;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiBossIdle : NakkiAiState {
	public override void AiUpdate(NakkiV2 nakki) { }
	public override void EnterState(NakkiV2 nakki) { }
	public override void ExitState(NakkiV2 nakki) { }
	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }
}
