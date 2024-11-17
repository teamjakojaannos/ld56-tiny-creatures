using System.Linq;

using Godot;

namespace Jakojaannos.WisperingWoods.Gameplay.Dialogue.UI;

[Tool]
public partial class BackgroundSmoke : Control {
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
	public float SmokeTimeOffsetHorizontal {
		get => _smokeTimeOffsetHorizontal;
		set {
			_smokeTimeOffsetHorizontal = value;
			var childMaterials = GetChildren()
				.OfType<CanvasItem>()
				.Select(child => child.Material)
				.OfType<ShaderMaterial>();
			foreach (var material in childMaterials) {
				material.SetShaderParameter("TimeOffsetHorizontal", _smokeTimeOffsetHorizontal);
			}
		}
	}
	private float _smokeTimeOffsetHorizontal = 0.0f;

	[Export]
	public float SmokeTimeOffsetVertical {
		get => _smokeTimeOffsetVertical;
		set {
			_smokeTimeOffsetVertical = value;
			var childMaterials = GetChildren()
				.OfType<CanvasItem>()
				.Select(child => child.Material)
				.OfType<ShaderMaterial>();
			foreach (var material in childMaterials) {
				material.SetShaderParameter("TimeOffsetVertical", _smokeTimeOffsetVertical);
			}
		}
	}
	private float _smokeTimeOffsetVertical = 0.0f;
}
