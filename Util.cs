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

	public static T TrustMeBro<T>() where T: class {
		if (!Engine.IsEditorHint()) {
			throw new System.InvalidOperationException("Value of a required exported field is null");
		}

		return null!;
	}

	public static T AssertNotNullOutsideEditor<T>(this Node node) where T: class {
		if (!Engine.IsEditorHint()) {
			throw new System.InvalidOperationException($"Node \"{node.Name}\" is not fully configured: Value of a required property is missing");
		}

		return null!;
	}

	public static IEnumerable<string> CheckCommonConfigurationWarnings(this Node node) {
		return node
            .GetType()
            .GetProperties()
            .Where(f => f.GetCustomAttribute<ExportAttribute>() is not null)
            .Where(f => f.GetCustomAttribute<MustSetInEditorAttribute>() is not null)
            .Where(field => field.GetValue(node) is null)
            .Select(field => $"{field.Name} is required but not set!")
            .ToArray();
	}
}