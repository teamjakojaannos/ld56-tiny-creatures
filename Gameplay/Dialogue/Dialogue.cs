using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class Dialogue : Node {
	[Export]
	public Godot.Collections.Array<DialogueLine> Lines { get; set; } = [];
}
