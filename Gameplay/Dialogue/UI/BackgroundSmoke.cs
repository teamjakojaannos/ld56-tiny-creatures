using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class BackgroundSmoke : Control {
	[Export(PropertyHint.Range, "0,1")]
	public float PercentageOffScreen {
		get => AnchorTop * 2.0f;
		set {
			AnchorTop = value / 2.0f;
			AnchorBottom = AnchorTop + 1.0f;
		}
	}

	[Export]
	public float SmokeTimeScale { get; set; } = 0.25f;

	private float _currentTime;

	public override void _Process(double delta) {
		base._Process(delta);

		_currentTime += SmokeTimeScale * (float)delta;

		var childMaterials = GetChildren()
				.OfType<CanvasItem>()
				.Select(child => child.Material)
				.OfType<ShaderMaterial>();
		foreach (var material in childMaterials) {
			material.SetShaderParameter("CurrentTime", _currentTime);
		}
	}
}
