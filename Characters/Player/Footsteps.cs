using System;
using System.Linq;

using Godot;

[Tool]
[Obsolete("Moved to Audio/RandomStreamPlayer")]
public partial class Footsteps : Node2D {
	private RandomNumberGenerator rng = new();

	public void Play() {
		if (GetChild(0) is AudioStreamPlayer2D) {
			var available = GetChildren()
				.OfType<AudioStreamPlayer2D>()
				.Where(stream => !stream.Playing)
				.ToList();

			var sfx = available.Count == 0
				// Fallback: just pick at random
				? GetChild<AudioStreamPlayer2D>(rng.RandiRange(0, GetChildCount() - 1))
				: available[rng.RandiRange(0, available.Count - 1)];

			sfx.Play();
		} else {
			var available = GetChildren()
				.OfType<AudioStreamPlayer>()
				.Where(stream => !stream.Playing)
				.ToList();

			var sfx = available.Count == 0
				// Fallback: just pick first
				? GetChild<AudioStreamPlayer>(0)
				: available[rng.RandiRange(0, available.Count - 1)];

			sfx.Play();
		}
	}
}
