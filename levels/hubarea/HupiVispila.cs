using Godot;

public partial class HupiVispila : Node2D {
	[Export]
	public string? Location;

	private bool saved = false;

	public override void _Ready() {
		base._Ready();
		Visible = false;

		var persistent = Persistent.Instance(this);

		persistent.WispSaved += (location) => {
			if (saved) {
				return;
			}

			if (Location == location) {
				saved = true;
				Visible = true;
				persistent.SavedCount++;
			}
		};
	}
}
