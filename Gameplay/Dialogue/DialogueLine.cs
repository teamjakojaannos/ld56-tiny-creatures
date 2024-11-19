using Godot;

using Jakojaannos.WisperingWoods.Characters;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue;

[Tool]
[GlobalClass]
public abstract partial class DialogueLine : Resource {
	[Export]
	public GameCharacter? Speaker { get; set; }

	[Export]
	public DialogueSide Side {
		get => _side ?? DefaultSide;
		set {
			_side = value;
			NotifyPropertyListChanged();
		}
	}
	private DialogueSide? _side;

	private DialogueSide DefaultSide => Speaker?.DefaultDialogueSide ?? DialogueSide.Left;

	public override bool _PropertyCanRevert(StringName property) {
		if (property == PropertyName.Side) {
			return Side != DefaultSide;
		}

		return base._PropertyCanRevert(property);
	}

	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.Side) {
			// FIXME: this flags false-positives
			return Variant.From(DefaultSide);
		}

		return base._PropertyGetRevert(property);
	}
}