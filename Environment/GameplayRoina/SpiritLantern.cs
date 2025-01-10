using Godot;

using Jakojaannos.WisperingWoods;

public partial class SpiritLantern : Node2D {
	[Export]
	public AnimationPlayer? AnimPlayer;

	[Export]
	public string AnimName = "out";

	[Export]
	public string Location = "hupialue";

	public override void _Ready() {
		AnimPlayer!.AnimationFinished += (animation) => {
			GD.Print($"Anim Finished: {animation}");
			if (animation == AnimName) {
				GD.Print($"Yayy!!!");
				this.Persistent().EmitSignal(Persistent.SignalName.WispSaved, Location);
			}
		};
	}
}
