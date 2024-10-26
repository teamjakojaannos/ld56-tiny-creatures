using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Audio;

[Tool]
[GlobalClass]
public partial class RandomAudioStreamPlayer2D : Node2D {
	public void Play() {
		var audioStreams = GetChildren()
			.OfType<AudioStreamPlayer>()
			.Select(PlayableAudioStream<AudioStreamPlayer>.From);
		RandomAudioStreamPlayer.PlayRandom(audioStreams);
	}
}
