using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Cinematic;

public static class CameraControlExtension {
	private static CameraControl? _instance;

	public static CameraControl MainCamera(this Node node) {
		return _instance is not null
			? _instance
			: (_instance = (CameraControl)node.GetTree().GetFirstNodeInGroup("MainCamera"));
	}
}

[GlobalClass]
public partial class CameraControl : Camera2D {
	[Export]
	public AudioStreamPlayer? ThudSfx;

	[Export]
	[ExportCategory("Prewire")]
	[MustSetInEditor]
	public Node Fader {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_fader);
		set => this.SetExportProperty(ref _fader, value);
	}
	private Node? _fader;

	private float shakeAmount = 0.0f;
	private float shakeFade = 0.0f;

	private RandomNumberGenerator rng = new();

	[Signal]
	public delegate void FadeFinishedEventHandler();

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

	public void SetFullyObscured() {
		Fader.Set("FadeProgress", 1.0f);
	}

	public void SetFullyVisible() {
		Fader.Set("FadeProgress", 0.0f);
	}

	public void FadeToBlack() {
		Fader.Set("FadeProgress", 0.0f);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(Fader, "FadeProgress", 1.0f, 2.5f);
		tween.SetEase(Tween.EaseType.InOut);
		tween.Finished += () => EmitSignal(SignalName.FadeFinished);
		tween.Play();
	}

	public void FadeToVisible() {
		Fader.Set("FadeProgress", 1.0f);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(Fader, "FadeProgress", 0.0f, 2.5f);
		tween.SetEase(Tween.EaseType.InOut);
		tween.Finished += () => EmitSignal(SignalName.FadeFinished);
		tween.Play();
	}
}
