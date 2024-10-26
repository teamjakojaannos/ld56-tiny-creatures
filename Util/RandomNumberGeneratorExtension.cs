using Godot;

namespace Jakojaannos.WisperingWoods.Util;

public static class RandomNumberGeneratorExtension {
	public static Vector2 RandomVector(this RandomNumberGenerator rng, (float, float) range) {
		return RandomVector(rng, range.Item1, range.Item2);
	}

	public static Vector2 RandomVector(this RandomNumberGenerator rng, float minDistance, float maxDistance) {
		var angle = rng.RandfRange(0, Mathf.Pi * 2.0f);
		var dist = rng.RandfRange(minDistance, maxDistance);

		return Vector2.FromAngle(angle) * dist;
	}

	public static bool DiceRoll(this RandomNumberGenerator rng, float chance) {
		return rng.Randf() <= chance;
	}

	public static bool RandomBool(this RandomNumberGenerator rng) {
		return rng.RandiRange(0, 1) == 0;
	}
}
