using Godot;

namespace Jakojaannos.WisperingWoods.Characters;

[Tool]
[GlobalClass]
public partial class GameCharacter : Resource {
	[Export]
	public string Name { get; set; } = "???";

	[Export]
	public Texture2D? Portrait { get; set; }
}
