using System.Linq;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

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

	public static void StalkOrAttack(
		NakkiV2 nakki,
		NakkiAttackState attackState,
		NakkiStalkState stalkState
	) {
		if (nakki._detectionLevel >= nakki._attackThreshold) {
			nakki.SwitchToState(attackState!);
			return;
		}

		if (nakki._detectionLevel >= nakki._stalkThreshold) {
			nakki.SwitchToState(stalkState!);
			return;
		}
	}

	private static readonly RandomNumberGenerator s_rng = new();

	public static bool TrySwitchToOneOf(NakkiV2 nakki, Array<NakkiAiState> states) {
		var possibleStates = states.Where(s => s.IsStateReady(nakki));

		s_rng.TryPickRandom(possibleStates, out var state);
		if (state != null) {
			nakki.SwitchToState(state);
			return true;
		}

		return false;
	}
}

public enum NakkiAnimation {
	Dive,
	EmergeFromWater,
	Attack,
}
