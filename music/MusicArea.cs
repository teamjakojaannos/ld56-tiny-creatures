using Godot;

public partial class MusicArea : Area2D {
	[Export]
	public Jukebox.MuzakTrack Music = Jukebox.MuzakTrack.SfwHub;

	public override void _Ready() {
		BodyEntered += (body) => {
			if (body is not Player player) {
				return;
			}

			// HACK: change footstep sounds to wet variant when moving to swamp
			if (Music == Jukebox.MuzakTrack.GetOut) {
				player.IsWet = true;
			} else {
				player.IsWet = false;
			}
			this.Jukebox().SwitchTrack(Music);
		};
	}
}
