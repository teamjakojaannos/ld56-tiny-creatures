using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.PlayerInput;

public interface IWispPointOfInterest {
	/// <summary>
	/// Global position where wisp is positioned while interacting with this
	/// point of interest.
	/// </summary>
	public Vector2 WispGlobalPosition { get; }

	public virtual bool IsInvisible {
		get => this is CanvasItem item && !item.Visible;
	}

	public virtual bool IsInactive {
		get => false;
	}

	public virtual float DistanceTo(Vector2 position) => WispGlobalPosition.DistanceTo(position);

	public interface IInteractable : IWispPointOfInterest {
	}

	public interface IInspectable : IWispPointOfInterest {
	}
}
