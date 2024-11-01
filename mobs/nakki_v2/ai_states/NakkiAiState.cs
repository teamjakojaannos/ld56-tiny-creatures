using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiAiState : Node {

	public virtual void EnterState(NakkiV2 nakki) {
	}

	public virtual void ExitState(NakkiV2 nakki) {
	}

	public virtual void AiUpdate(NakkiV2 nakki) {
	}

	public virtual bool ShouldTickDetection() {
		return true;
	}

	public virtual void DetectionLevelChanged(NakkiV2 nakki) {
		if (nakki._detectionLevel >= nakki._attackThreshold) {
			nakki.EnterAttackState();
			return;
		}

		if (nakki._detectionLevel >= nakki._alertThreshold) {
			nakki.EnterAlertState();
			return;
		}
	}
}
