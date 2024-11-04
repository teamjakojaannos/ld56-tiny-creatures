using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class Dialogue : Node {
	public Godot.Collections.Array<DialogueLine> Lines =>
		new(GetChildren().OfType<DialogueLine>());
}
