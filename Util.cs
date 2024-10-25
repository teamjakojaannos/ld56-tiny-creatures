using Godot;

public static class Util {
	public static Vector2 randomVector(RandomNumberGenerator rng, (float, float) range) {
		return randomVector(rng, range.Item1, range.Item2);
	}

	public static Vector2 randomVector(RandomNumberGenerator rng, float minDistance, float maxDistance) {
		var angle = rng.RandfRange(0, Mathf.Pi * 2.0f);
		var dist = rng.RandfRange(minDistance, maxDistance);

		return Vector2.FromAngle(angle) * dist;
	}

	public static bool diceRoll(RandomNumberGenerator rng, float chance) {
		return rng.Randf() <= chance;
	}

	public static bool randomBool(RandomNumberGenerator rng) {
		return rng.RandiRange(0, 1) == 0;
	}
}