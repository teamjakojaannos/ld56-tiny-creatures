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

	[Export]
	public bool OneShot = false;

	private bool _done = false;
	public bool Done {
		get => _done;
		set {
			_done = value;
			if (_done) {
				isWispInteracting = false;
				if (GetTree().GetFirstNodeInGroup("Player") is Player player) {
					player.WispTarget = null;
				}
				wisp = null;

				StopInteract();
			}
		}
	}

	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();
		if (Target is null) {
			warnings = warnings.Append("Target is not set!").ToArray();
		}

		return warnings;
	}

	private Node2D? wisp;
	public bool isWispInteracting = false;
	private bool isFirstTimeStart = true;
	private bool isFirstTimeStop = true;

	[Export]
	public string RequireState = "";

	[Export]
	public string DisableIfState = "";

	[Signal]
	public delegate void InteractStartEventHandler();

	[Signal]
	public delegate void InteractStopEventHandler();

	private bool RequirementsMet() {
		return this.Persistent().State.Contains(RequireState.Trim());
	}

	private bool IsDisabledByState() {
		return !this.Persistent().State.Contains(DisableIfState.Trim());
	}

	public override void _Ready() {
		base._Ready();

		if (!Engine.IsEditorHint()) {
			BodyEntered += (body) => {
				if (Done || !RequirementsMet() || IsDisabledByState()) {
					return;
				}

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
		if (Done) {
			return;
		}

		if (!isFirstTimeStart && OneShot) {
			return;
		}
		isFirstTimeStart = false;

		EmitSignal(SignalName.InteractStart);
	}

	public virtual void StopInteract() {
		if (Done) {
			return;
		}

		if (!isFirstTimeStop && OneShot) {
			return;
		}
		isFirstTimeStop = false;

		EmitSignal(SignalName.InteractStop);
	}
}
