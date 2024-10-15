using System;
using System.Collections.Generic;
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
			throw new InvalidOperationException("Value of a required exported field is null");
		}

		return null!;
	}

	public static T EnsureChildExists<T>(this Node node, string name, Func<T> constructor) where T: Node {
		if (!node.HasNode(name)) {
			var newChild = constructor();
			newChild.Name = name;

			node.AddChild(newChild, forceReadableName: true);
			newChild.Owner = node;
		}

		var child = node.GetNode<T>(name);
		if (child is not T t) {
			throw new InvalidOperationException($"Node {node.Name} has a child with {name}, but unexpected type {child.GetType().Name} (expected: {typeof(T)})");
		}

		return t;
	}

	public static IEnumerable<T> FindDescendantsOfType<T>(this Node node) {
		var descendants = new List<T>();
		foreach (var child in node.GetChildren()) {
			if (child is T matching) {
				descendants.Add(matching);
			}

			var transitiveDescendants = child.FindDescendantsOfType<T>();
			descendants.AddRange(transitiveDescendants);
		}

		return descendants;
	}

	public static T? FindParentOfTypeOrNull<T>(this Node node) where T: class {
		Node parent = node.GetParent();
		while (parent is not T) {
			parent = parent.GetParent();
		}

		return parent as T;
	}
}