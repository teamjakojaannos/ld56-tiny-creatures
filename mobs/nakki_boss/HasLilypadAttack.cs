namespace Jakojaannos.WisperingWoods;

public interface HasLilypadAttack {
	public LilypadAttackStats GetAttackStats();
	public void LilypadAttackWasCompleted(int attackId);
}

public class LilypadAttackStats {

	public readonly int? AttackId;

	public LilypadAttackStats() { }

	public LilypadAttackStats(int attackId) {
		AttackId = attackId;
	}

	public static LilypadAttackStats Default() {
		return new();
	}


	private static int s_id = 0;
	public static int GenerateId() {
		return s_id++;
	}
}
