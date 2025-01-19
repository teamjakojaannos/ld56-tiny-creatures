using Godot;

using Jakojaannos.WisperingWoods.Gameplay.PlayerInput;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay;

[Tool]
[GlobalClass]
[RequireParent(typeof(Area2D))]
public partial class InspectText : Node, IWispPointOfInterest.IInspectable {
	private bool _isHovered = false;

	public Vector2 WispGlobalPosition => GetParent<Area2D>().GlobalPosition;

	public override string[] _GetConfigurationWarnings() => this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings());

	public override void _Ready() {
		var area = GetParent<Area2D>();
		area.MouseEntered += () => this.InteractionController().TrackPointOfInterest(this);
		area.MouseExited += () => this.InteractionController().UntrackPointOfInterest(this);
	}
}
