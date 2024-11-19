using Godot;

using Jakojaannos.WisperingWoods.Characters;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public partial class DialogueOption : Resource {
	[Export]
	public string Text { get; set; } = "This is an option";

	[Export]
	public GameCharacter? Character { get; set; } = null;

	[Export]
	public bool OverrideCharacter {
		get => Character != null || _overrideCharacter;
		set {
			if (!value) {
				Character = null;
			}
			_overrideCharacter = value;
			NotifyPropertyListChanged();
		}
	}
	private bool _overrideCharacter = false;

	public override void _ValidateProperty(Godot.Collections.Dictionary property) {
		base._ValidateProperty(property);

		var propertyName = property["name"].AsStringName();
		if (propertyName == PropertyName.OverrideCharacter) {
			property["usage"] = (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.NoInstanceState);
		} else if (propertyName == PropertyName.Character) {
			if (!OverrideCharacter) {
				property["usage"] = (int)PropertyUsageFlags.None;
			}
		}
	}
}
