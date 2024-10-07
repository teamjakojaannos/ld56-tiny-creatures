using Godot;

public partial class MusicArea : Area2D {
	[Export]
	public Jukebox.MuzakTrack Music = Jukebox.MuzakTrack.SfwHub;

	public override void _Ready() {
		BodyEntered += (body) => {
			if (body is not Player player) {
				return;
			}

			this.Jukebox().SwitchTrack(Music);
		};
	}
}
