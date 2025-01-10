using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class WispInteractable : Area2D {
	[Export]
	[MustSetInEditor]
	public Node2D Target {
		get => _target ?? this.GetNotNullExportPropertyWithNullableBackingField(_target);
		set => this.SetExportProperty(ref _target, value);
	}
	private Node2D? _target;

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
		return (base._GetConfigurationWarnings() ?? System.Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	private Node2D? wisp;
	public bool isWispInteracting = false;
	private bool isFirstTimeStart = true;
	private bool isFirstTimeStop = true;

	[Export]
	public string RequireState = "";

	[Export]
	public string DisableIfState = "";

	[Export]
	public int RequireNumberOfWispsGreaterThan = -1;

	[Export]
	public int RequireNumberOfWispsLessThan = -1;

	[Export]
	public bool HideIfDisabledOrRequirementsNotMet = true;

	[Signal]
	public delegate void InteractStartEventHandler();

	[Signal]
	public delegate void InteractStopEventHandler();

	private bool RequirementsMet() {
		bool state = RequireState.Trim().Length == 0 || this.Persistent().State.Contains(RequireState.Trim());
		bool wispsGreat = RequireNumberOfWispsGreaterThan == -1 || this.Persistent().SavedCount > RequireNumberOfWispsGreaterThan;
		bool wispsLess = RequireNumberOfWispsLessThan == -1 || this.Persistent().SavedCount < RequireNumberOfWispsLessThan;
		return state && wispsGreat && wispsLess;
	}

	private bool IsDisabledByState() {
		return DisableIfState.Trim().Length != 0 && this.Persistent().State.Contains(DisableIfState.Trim());
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

		if (Engine.IsEditorHint()) {
			return;
		}

		if (HideIfDisabledOrRequirementsNotMet) {
			if (!RequirementsMet() || IsDisabledByState()) {
				Visible = false;
				return;
			}

			Visible = true;
		}

		if (wisp is null) {
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
