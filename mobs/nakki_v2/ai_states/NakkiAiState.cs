using System.Collections.Generic;

using Godot;

namespace Jakojaannos.WisperingWoods;

public abstract partial class NakkiAiState : Node {
	public virtual bool IsStateReady(NakkiV2 nakki) {
		return true;
	}
	public abstract void EnterState(NakkiV2 nakki);
	public abstract void ExitState(NakkiV2 nakki);
	public abstract void AiUpdate(NakkiV2 nakki);
	public abstract bool ShouldTickDetection();
	public abstract void DetectionLevelChanged(NakkiV2 nakki);
	public virtual void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) { }
	public virtual void ReceiveTrigger(NakkiV2 nakki) { }
}

public enum NakkiAnimation {
	Dive,
	EmergeFromWater,
	Attack,
}
