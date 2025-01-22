using System.Collections.Generic;
using System.Linq;

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

	public static bool TryPickRandom<T>(this RandomNumberGenerator rng, IList<T> list, out T? value) {
		if (list.Count == 0) {
			value = default;
			return false;
		}

		value = rng.PickRandomUnchecked(list);
		return true;
	}

	public static bool TryPickRandom<T>(this RandomNumberGenerator rng, IEnumerable<T> values, out T? value) {
		if (values.Any()) {
			value = rng.PickRandomUnchecked(values);
			return true;
		}

		value = default;
		return false;
	}

	public static T PickRandomUnchecked<T>(this RandomNumberGenerator rng, IList<T> list) {
		return list[rng.RandiRange(0, list.Count - 1)];
	}

	public static T PickRandomUnchecked<T>(this RandomNumberGenerator rng, IEnumerable<T> values) {
		return values
			.Skip(rng.RandiRange(0, values.Count() - 1))
			.First();
	}

	public static float ApplyRandomVariation(this RandomNumberGenerator rng, float value, float variationPercent) {
		var variation = rng.RandfRange(-variationPercent, variationPercent);
		return value * (1.0f + variation);
	}
}
