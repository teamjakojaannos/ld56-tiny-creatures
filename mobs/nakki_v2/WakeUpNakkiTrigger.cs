using Godot;


namespace Jakojaannos.WisperingWoods;

public partial class WakeUpNakkiTrigger : Area2D {
	[Export] private NakkiV2? _nakkiToTrigger;

	public override void _Ready() {
		if (_nakkiToTrigger == null) {
			GD.PrintErr("Näkki trigger's target is not set!");
		}

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D node) {
		if (node is Player) {
			_nakkiToTrigger?.PlayerEnteredTrigger();
		}
	}
}
