using System.Linq;
using Godot;

[Tool]
[GlobalClass]
public partial class WispInteractableAnimatedProp : Node2D {
    private AnimationPlayer? _animationPlayer;

    [Export]
    public AnimationPlayer AnimPlayer {
        get => _animationPlayer ?? Util.TrustMeBro<AnimationPlayer>();
        set {
            _animationPlayer = value;
            UpdateConfigurationWarnings();
        }
    }

    private string? _animation;

    [Export]
    public string Animation {
        get => _animation ?? Util.TrustMeBro<string>();
        set {
            _animation = value;
            UpdateConfigurationWarnings();
        }
    }

    public override string[] _GetConfigurationWarnings() {
        var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();

        if (GetParentOrNull<WispInteractable>() is null) {
            warnings = warnings.Append("Parent must be a WispInteractable!").ToArray();
        }

        if (_animationPlayer is null) {
            warnings = warnings.Append("AnimationPlayer is not set!").ToArray();
        }

        if (_animation is null || _animation.Trim().Length == 0) {
            warnings = warnings.Append("Animation is empty or not set!").ToArray();
        }

        return warnings;
    }

	public override void _Ready() {
		base._Ready();

        if (Engine.IsEditorHint()) {
            return;
        }

        var parent = GetParent<WispInteractable>();
        parent.InteractStart += () => AnimPlayer.Play(Animation);
        parent.InteractStop += () => AnimPlayer.Stop();
	}
}
