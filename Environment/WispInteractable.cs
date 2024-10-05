using System.Linq;
using Godot;

[Tool]
public partial class WispInteractable : Area2D {
	private Node2D? _target;

	[Export]
	public Node2D Target {
		get => _target ?? Util.TrustMeBro<Node2D>();
		set {
			_target = value;
			UpdateConfigurationWarnings();
		}
	}

	[Export]
	public float GoalDistance = 16.0f;

	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();
		if (Target is null) {
			warnings = warnings.Append("Target is not set!").ToArray();
		}

		return warnings;
	}

	private Node2D? wisp;
	private bool isWispInteracting = false;

	[Signal]
	public delegate void InteractStartEventHandler();

	[Signal]
	public delegate void InteractStopEventHandler();

	public override void _Ready() {
		base._Ready();

		if (!Engine.IsEditorHint()) {
			BodyEntered += (body) => {
				if (body is Player player) {
					player.WispTarget = Target;
					wisp = player.Wisp;
				}
			};

			BodyExited += (body) => {
				if (body is Player player) {
					if (isWispInteracting) {
						GetTree().CreateTimer(1.0f).Timeout += () => {
							isWispInteracting = false;
							player.WispTarget = null;
							wisp = null;

							StopInteract();
						};
					} else {
						player.WispTarget = null;
						wisp = null;
					}
				}
			};
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint() || wisp is null) {
			return;
		}

		var distance = wisp.GlobalPosition.DistanceTo(Target.GlobalPosition);
		if (distance < GoalDistance && !isWispInteracting) {
			isWispInteracting = true;
			StartInteract();
		}
	}

	public virtual void StartInteract() {
	}

	public virtual void StopInteract() {
	}
}
