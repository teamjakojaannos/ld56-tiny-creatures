using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Audio;

[Tool]
[GlobalClass]
public partial class RandomAudioStreamPlayer2D : Node2D {
	public void Play() {
		var audioStreams = GetChildren()
			.OfType<AudioStreamPlayer2D>()
			.Select(PlayableAudioStream<AudioStreamPlayer2D>.From);
		RandomAudioStreamPlayer.PlayRandom(audioStreams);
	}
}
