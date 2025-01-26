using Godot;

namespace Jakojaannos.WisperingWoods;

[Tool]
public abstract partial class NakkiBossStage : NakkiAiState {
	public abstract void LilypadAttackWasCompleted(int attackId);
	[Signal] public delegate void LilypadAttackInitiatedEventHandler(LilypadAttackStats stats);
}
