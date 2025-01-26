using Godot;

namespace Jakojaannos.WisperingWoods;

public partial class LilypadAttackStats : Resource {
	public float UnderwaterTime { get; set; } = 1.5f;
	public float UnderwaterTimeVariation { get; set; } = 0.5f;
	public float SinkSpeed { get; set; } = 1.0f;
	public float SinkSpeedVariation { get; set; } = 0.5f;
	public float ShakeTime { get; set; } = 0.75f;
	public float ShakeTimeVariation { get; set; } = 0.5f;
	public int? AttackId { get; set; } = null;


	public static LilypadAttackStats Default() {
		return new();
	}


	private static int s_id = 0;
	public static int GenerateId() {
		return s_id++;
	}
}
