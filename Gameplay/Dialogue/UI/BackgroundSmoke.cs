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
	public float SmokeTimeScale {
		get => _smokeTimeScale;
		set {
			_smokeTimeScale = value;
			var childMaterials = GetChildren()
				.OfType<CanvasItem>()
				.Select(child => child.Material)
				.OfType<ShaderMaterial>();
			foreach (var material in childMaterials) {
				material.SetShaderParameter("TimeScale", _smokeTimeScale);
			}
		}
	}
	private float _smokeTimeScale = 0.25f;

	[Export]
	public float SmokeTimeOffset {
		get => _smokeTimeOffset;
		set {
			_smokeTimeOffset = value;
			var childMaterials = GetChildren()
				.OfType<CanvasItem>()
				.Select(child => child.Material)
				.OfType<ShaderMaterial>();
			foreach (var material in childMaterials) {
				material.SetShaderParameter("TimeOffset", _smokeTimeOffset);
			}
		}
	}
	private float _smokeTimeOffset = 0.0f;
}
