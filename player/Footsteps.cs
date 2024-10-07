using System.Linq;
using Godot;

[Tool]
public partial class Footsteps : Node {
	private RandomNumberGenerator rng = new();

	public void Play() {
		var available = GetChildren()
			.OfType<AudioStreamPlayer>()
			.Where(stream => !stream.Playing)
			.ToList();

		var sfx = available.Count ==  0
			// Fallback: just pick first
			? GetChild<AudioStreamPlayer>(0)
			: available[rng.RandiRange(0, available.Count - 1)];

		sfx.Play();
	}
}
