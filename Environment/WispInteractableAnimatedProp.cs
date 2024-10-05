using System.Linq;
using Godot;

[Tool]
public partial class WispInteractableAnimatedProp : WispInteractable {
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
        if (_animationPlayer is null) {
            warnings = warnings.Append("AnimationPlayer is not set!").ToArray();
        }

        if (_animation is null || _animation.Trim().Length == 0) {
            warnings = warnings.Append("Animation is empty or not set!").ToArray();
        }

        return warnings;
    }

    public override void StartInteract() {
        AnimPlayer.Play(Animation);
    }

	public override void StopInteract() {
		AnimPlayer.Stop();
	}
}
