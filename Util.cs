using Godot;

public static class Util {
	public static Vector2 randomVector(RandomNumberGenerator rng, float minDistance, float maxDistance) {
		var angle = rng.RandfRange(0, Mathf.Pi * 2.0f);
		var dist = rng.RandfRange(minDistance, maxDistance);

		return Vector2.FromAngle(angle) * dist;
	}

	public static bool randomBool(RandomNumberGenerator rng) {
		return rng.RandiRange(0, 1) == 0;
	}

	public static T TrustMeBro<T>() where T: class {
		if (!Engine.IsEditorHint()) {
			throw new System.InvalidOperationException("Value of a required exported field is null");
		}

		return null!;
	}
}