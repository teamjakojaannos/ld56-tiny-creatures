using Godot;

public static class CameraControlExtension {
	private static CameraControl? _instance;

	public static CameraControl MainCamera(this Node node) {
		return _instance is not null
			? _instance
			: (_instance = (CameraControl)node.GetTree().GetFirstNodeInGroup("MainCamera"));
	}
}

public partial class CameraControl : Camera2D {
	[Export]
	public AudioStreamPlayer? ThudSfx;

	private float shakeAmount = 0.0f;
	private float shakeFade = 0.0f;

	private RandomNumberGenerator rng = new();

	public override void _Process(double _delta) {
		var delta = (float)_delta;

		if (shakeAmount > 0.0f) {
			shakeAmount = Mathf.MoveToward(shakeAmount, 0.0f, shakeFade * delta);
			Offset = new(
				rng.RandfRange(-shakeAmount, shakeAmount),
				rng.RandfRange(-shakeAmount, shakeAmount)
			);
		} else {
			Offset = Vector2.Zero;
		}
	}

	public void ApplyCameraShake(float strength, float fade = 7.5f) {
		shakeAmount = strength;
		shakeFade = fade;
		ThudSfx?.Play();
	}
}
