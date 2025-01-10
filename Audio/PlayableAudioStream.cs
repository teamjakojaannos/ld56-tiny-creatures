using System;

using Godot;

namespace Jakojaannos.WisperingWoods.Audio;

/// <summary>
/// Lightweight generic wrapper for abstracting away whether or not an <code>
/// AudioStreamPlayer</code> is positional.
/// </summary>
public readonly struct PlayableAudioStream<TStreamPlayer> {
	public bool IsPlaying => _isPlaying.Invoke(_wrapped);

	public void Play() {
		_play.Invoke(_wrapped);
	}

	private readonly TStreamPlayer _wrapped;

	// These are defined as `T => void` instead of `() => void` on purpose.
	// This way, the lambdas don't need to capture the wrapped stream player.
	private readonly Action<TStreamPlayer> _play;
	private readonly Func<TStreamPlayer, bool> _isPlaying;

	internal PlayableAudioStream(TStreamPlayer wrapped, Action<TStreamPlayer> play, Func<TStreamPlayer, bool> isPlaying) {
		_wrapped = wrapped;
		_play = play;
		_isPlaying = isPlaying;
	}

	public static PlayableAudioStream<AudioStreamPlayer> From(AudioStreamPlayer streamPlayer) {
		return new(streamPlayer, p => p.Play(), p => p.IsPlaying());
	}

	public static PlayableAudioStream<AudioStreamPlayer2D> From(AudioStreamPlayer2D streamPlayer) {
		return new(streamPlayer, p => p.Play(), p => p.IsPlaying());
	}
}
