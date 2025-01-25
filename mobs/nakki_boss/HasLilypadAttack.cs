namespace Jakojaannos.WisperingWoods;

public interface HasLilypadAttack {
	public LilypadAttackStats GetAttackStats();
	public void LilypadAttackWasCompleted(int attackId);
}

public record LilypadAttackStats(
		float UnderwaterTime = 1.5f,
		float UnderwaterTimeVariation = 0.5f,
		float SinkSpeed = 1.0f,
		float SinkSpeedVariation = 0.5f,
		float ShakeTime = 0.75f,
		float ShakeTimeVariation = 0.5f,
		int? AttackId = null
	) {

	public static LilypadAttackStats Default() {
		return new();
	}


	private static int s_id = 0;
	public static int GenerateId() {
		return s_id++;
	}
}
