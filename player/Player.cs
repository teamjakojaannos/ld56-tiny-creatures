using System;
using System.Linq;
using Godot;

[Tool]
public partial class Player : CharacterBody2D {
	[Signal]
	public delegate void LightLevelChangedEventHandler(int newLightLevel);

	[Export]
	public float Speed = 300.0f;

	[Export]
	public float Friction = 10.0f;

	[Export]
	public AnimationPlayer? Animation;

	public bool IsAllowedToMove => !Dialogue.Instance(this).Visible && !frozen;

	private bool frozen = false;

	public Node2D? WispTarget { get; set; } = null;

	private Node2D? _wispFollowNode;

	[Export]
	public Footsteps? Footsteps;

	[Export]
	public Footsteps? FootstepsWet;

	public bool IsWet = false;

	[Export]
	public Timer? FootstepsTimer;

	[Export]
	public Node2D WispFollowNode {
		get => _wispFollowNode ?? Util.TrustMeBro<Node2D>();
		set {
			_wispFollowNode = value;
			UpdateConfigurationWarnings();
		}
	}

	private Node2D? _wisp;

	[Export]
	public Node2D Wisp {
		get => _wisp ?? Util.TrustMeBro<Node2D>();
		set {
			_wisp = value;
			UpdateConfigurationWarnings();
		}
	}

	public bool Slowed { get; internal set; } = false;

	private AnimatedSprite2D? playerSprite;

	public int lightLevel = 1;
	private const int lightLevelMin = 1;
	private const int lightLevelMax = 3;

	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? Array.Empty<string>();
		if (_wispFollowNode is null) {
			warnings = warnings.Append("WispFollowNode is not set!").ToArray();
		}

		if (_wisp is null) {
			warnings = warnings.Append("Wisp is not set!").ToArray();
		}

		return warnings;
	}

	[Signal]
	public delegate void ReadyToGoEventHandler();

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint()) {
			return;
		}

		playerSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		var mainPlayerSpawn = GetTree().GetFirstNodeInGroup("IntroPlayerSpawn");
		if (mainPlayerSpawn is Node2D spawn) {
			CallDeferred(MethodName.TeleportAt, spawn);
		} else {
			var spawns = GetTree().GetNodesInGroup("PlayerSpawn");
			if (spawns.Count != 0 && spawns.PickRandom() is Node2D fallbackRandomSpawn) {
				CallDeferred(MethodName.TeleportAt, fallbackRandomSpawn);
			}
		}

		if (FootstepsTimer is not null) {
			FootstepsTimer.Timeout += () => {

				if (IsWet) {
					FootstepsWet?.Play();
				} else {
					Footsteps?.Play();
				}
			};
		}
	}

	private void TeleportAt(Node2D target) {
		if (!target.IsInsideTree() || target.IsQueuedForDeletion()) {
			throw new InvalidOperationException("Tried teleporting to a non-existent/deleted node!");
		}

		GetParent()?.RemoveChild(this);
		(target.GetParent() ?? target).AddChild(this);

		GlobalPosition = target.GlobalPosition;

		EmitSignal(SignalName.ReadyToGo);
	}

	private string animationDirection = "Down";
	public override void _PhysicsProcess(double _delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)_delta;

		var direction = IsAllowedToMove
			? Input.GetVector("left", "right", "up", "down")
			: Vector2.Zero;

		var modifier = Slowed ? 0.5f : 1.0f;
		if (direction.LengthSquared() > 0.001f) {
			Velocity = direction * Speed * delta * modifier;
			animationDirection = direction.X < 0.0
				? "Left"
				: direction.X > 0.0
				? "Right"
				: direction.Y < 0.0
				? "Up"
				: "Down";

			Animation?.Play($"Walk{animationDirection}");

			if (FootstepsTimer is not null && FootstepsTimer.IsStopped()) {
				FootstepsTimer.Start();
			}
		} else {
			var currentSpeed = Velocity.Length();
			Velocity = Velocity.MoveToward(Vector2.Zero, currentSpeed * Friction * delta);
		}

		if (Velocity.LengthSquared() < 0.01f) {
			FootstepsTimer?.Stop();
			Animation?.Play($"Idle{animationDirection}");
		}

		var wispPosition = Wisp.GlobalPosition;
		MoveAndCollide(Velocity);

		// HACK: Cancel out wisp movement to emulate top-level movement.
		//       Can't use TopLevel=true as that breaks Y-sort.
		Wisp.GlobalPosition = wispPosition;
		if (WispTarget is not null) {

			var distance = Wisp.GlobalPosition.DistanceTo(WispTarget.GlobalPosition);
			Wisp.GlobalPosition =
				Wisp.GlobalPosition.MoveToward(WispTarget.GlobalPosition, distance * 2.0f * delta);
		} else {
			var distance = Wisp.GlobalPosition.DistanceTo(WispFollowNode.GlobalPosition);
			Wisp.GlobalPosition =
				Wisp.GlobalPosition.MoveToward(WispFollowNode.GlobalPosition, distance * 5f * delta);
		}
	}

	public void setSpriteVisible(bool visible) {
		playerSprite!.Visible = visible;
	}

	public void setMovementEnabled(bool enabled) {
		frozen = !enabled;
	}

	public void die() {
		GD.Print("I am dead.");
	}

	public void SetupForIntro(Node2D wispLocation) {
		setSpriteVisible(false);
		setMovementEnabled(false);
		WispTarget = wispLocation;
		Wisp.GlobalPosition = wispLocation.GlobalPosition;
	}

	public override void _Input(InputEvent inputEvent) {
		if (inputEvent.IsActionPressed("light_level_up")) {
			addLightLevel(+1);
			return;
		}

		if (inputEvent.IsActionPressed("light_level_down")) {
			addLightLevel(-1);
			return;
		}
	}

	private void addLightLevel(int amount) {
		lightLevel += amount;
		lightLevel = Mathf.Clamp(lightLevel, lightLevelMin, lightLevelMax);

		EmitSignal(SignalName.LightLevelChanged, lightLevel);
	}
}
