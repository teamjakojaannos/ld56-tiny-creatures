using System.Collections.Generic;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods.Audio;

[Tool]
[GlobalClass]
public partial class RandomAudioStreamPlayer : Node2D {
	public void Play() {
		var audioStreams = GetChildren()
			.OfType<AudioStreamPlayer>()
			.Select(PlayableAudioStream<AudioStreamPlayer>.From);
		PlayRandom(audioStreams);
	}

	private readonly static RandomNumberGenerator s_rng = new();

	internal static void PlayRandom<T>(IEnumerable<PlayableAudioStream<T>> audioStreams, RandomNumberGenerator? rng = null) where T : class {
		var idleStreams = audioStreams
			.Where(stream => !stream.IsPlaying)
			.ToList();

		var streamFound = idleStreams.Count != 0
			// Pick random idle audio stream
			? (rng ?? s_rng).TryPickRandom(idleStreams, out var selectedStream)
			// No idle streams available => fall back to just picking at random
			: (rng ?? s_rng).TryPickRandom(audioStreams, out selectedStream);
		if (streamFound) {
			selectedStream.Play();
		} else {
			GD.PrintErr($"Cannot play random audio stream: No audio streams available!");
		}
	}
}
