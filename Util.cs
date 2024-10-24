using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	[Obsolete("Short version: use extension methods from `ExportProp.cs`. Longer version: As funny as this was as a joke during LD, the name is somewhat inherently confusing. Additionally, this is better implemented as an extension on Node to support better diagnostics messages when values end up missing.")]
	public static T TrustMeBro<T>() where T: class {
		if (!Engine.IsEditorHint()) {
			throw new System.InvalidOperationException("Value of a required exported field is null");
		}

		return null!;
	}
}