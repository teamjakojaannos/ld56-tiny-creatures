using System;
using System.Collections.Generic;

using Godot;

public static class JukeboxExt {
	private static Jukebox? _instance;

	public static Jukebox Jukebox(this Node node) {
		return _instance is not null
			? _instance
			: (_instance = node.GetTree().Root.GetNode<Jukebox>("/root/Jukebox"));
	}
}

public partial class Jukebox : Node2D {
	[Export]
	[ExportCategory("Config")]
	public float FadeInSpeed = 0.05f;

	[Export]
	public float FadeOutSpeed = 0.5f;

	[Export]
	[ExportGroup("Tracks")]
	public AudioStreamPlayer? Metsämusa;
	[Export]
	public AudioStreamPlayer? Pornhub;
	[Export]
	public AudioStreamPlayer? Swhamp;
	[Export]
	public AudioStreamPlayer? Psykoosi;
	[Export]
	public AudioStreamPlayer? Credits;

	[Export]
	public AudioStreamPlayer? Combat;

	public enum MuzakTrack {
		ForrestGump,
		SfwHub,
		GetOut,
		Psykoosi,
		Credits,
	}

	private AudioStreamPlayer? GetStreamPlayer(MuzakTrack muzak) {
		return muzak switch {
			MuzakTrack.ForrestGump => Metsämusa,
			MuzakTrack.SfwHub => Pornhub,
			MuzakTrack.GetOut => Swhamp,
			MuzakTrack.Psykoosi => Psykoosi,
			MuzakTrack.Credits => Credits,
			_ => throw new NotImplementedException(),
		};
	}

	private static IEnumerable<MuzakTrack> AllTracks => Enum.GetValues<MuzakTrack>();

	private MuzakTrack? currentTrack = MuzakTrack.SfwHub;

	private bool isInCombat = false;

	public void StartChase() {
		isInCombat = true;

		if (!Combat!.Playing) {
			Combat.Play();
		}
	}

	public void StopChase() {
		isInCombat = false;
	}

	public override void _Ready() {
		base._Ready();

		foreach (var track in AllTracks) {
			var stream = GetStreamPlayer(track);
			if (stream is null) {
				GD.PushError($"Missing track {track}!");
				continue;
			}

			stream.VolumeDb = Mathf.LinearToDb(0.0f);
			if (track == currentTrack) {
				stream.Play();
			}
		}
	}

	public void SwitchTrack(MuzakTrack muzak) {
		if (currentTrack == MuzakTrack.Credits) {
			return;
		}

		currentTrack = muzak;

		foreach (var track in AllTracks) {
			var stream = GetStreamPlayer(track);
			if (stream is null) {
				GD.PushError($"Missing track {track}!");
				continue;
			}

			if (track == muzak && !stream.Playing) {
				stream.Play();
			}
		}
	}

	public override void _Process(double _delta) {
		base._Process(_delta);
		var delta = (float)_delta;

		foreach (var track in AllTracks) {
			var stream = GetStreamPlayer(track);
			if (stream is null) {
				continue;
			}

			var isCurrentTrack = !isInCombat && track == currentTrack;

			var targetVolume = isCurrentTrack ? 1.0f : 0.0f;
			var fadeSpeed = isCurrentTrack ? FadeInSpeed : FadeOutSpeed;

			var volume = Mathf.DbToLinear(stream.VolumeDb);
			var adjusted = Mathf.MoveToward(volume, targetVolume, fadeSpeed * delta);
			stream.VolumeDb = Mathf.LinearToDb(adjusted);

			if (track != currentTrack && adjusted < 0.001f) {
				stream.Stop();
			}
		}

		var isCurrentTrack2 = isInCombat;
		var targetVolume2 = isCurrentTrack2 ? 1.0f : 0.0f;
		var fadeSpeed2 = isCurrentTrack2 ? FadeInSpeed : FadeOutSpeed;

		var stream2 = Combat!;
		var volume2 = Mathf.DbToLinear(stream2.VolumeDb);
		var adjusted2 = Mathf.MoveToward(volume2, targetVolume2, fadeSpeed2 * delta);
		stream2.VolumeDb = Mathf.LinearToDb(adjusted2);

		if (adjusted2 < 0.0001f) {
			stream2!.Stop();
		}
	}
}
