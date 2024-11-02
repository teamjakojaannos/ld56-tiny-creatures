using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods;

public abstract partial class NakkiAiState : Node {

	public abstract string StateName();
	public abstract HashSet<string> RequiresStates();
	public abstract void EnterState(NakkiV2 nakki);
	public abstract void ExitState(NakkiV2 nakki);
	public abstract void AiUpdate(NakkiV2 nakki);

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
