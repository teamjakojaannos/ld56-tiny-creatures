using System;
using System.Linq;

using Godot;

using Jakojaannos.CodeGen;

using Jakojaannos.WisperingWoods.Audio;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Characters.Player;

[Tool]
public partial class Player : CharacterBody2D {
	[Signal]
	public delegate void LightLevelChangedEventHandler(int newLightLevel);

	[Export]
	public float Speed = 300.0f;

	[Export]
	public float Friction = 10.0f;

	[Export]
	[MustSetInEditor]
	public AnimationPlayer? Animation {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animation);
		set => this.SetExportProperty(ref _animation, value);
	}
	private AnimationPlayer? _animation;

	public bool IsAllowedToMove => !Dialogue.Instance(this).Visible && !frozen && !IsInCinematic;

	private bool frozen = false;

	public Node2D? WispTarget { get; set; } = null;

	[Export]
	[MustSetInEditor]
	public RandomAudioStreamPlayer2D Footsteps {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footsteps);
		set => this.SetExportProperty(ref _footsteps, value);
	}
	public RandomAudioStreamPlayer2D? _footsteps;

	[Export]
	[MustSetInEditor]
	public RandomAudioStreamPlayer2D FootstepsWet {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footstepsWet);
		set => this.SetExportProperty(ref _footstepsWet, value);
	}
	public RandomAudioStreamPlayer2D? _footstepsWet;

	public bool IsWet = false;

	[Export]
	public Timer? FootstepsTimer;

	[Export]
	public Sprite2D? Shadow;

	[Export]
	[MustSetInEditor]
	public Node2D WispFollowNode {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_wispFollowNode);
		set => this.SetExportProperty(ref _wispFollowNode, value);
	}
	private Node2D? _wispFollowNode;

	[Export]
	[MustSetInEditor]
	public Node2D Wisp {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_wisp);
		set => this.SetExportProperty(ref _wisp, value);
	}
	private Node2D? _wisp;

	public bool Slowed { get; internal set; } = false;

	private bool _isInCinematic = false;
	public bool IsInCinematic {
		get => _isInCinematic;
		internal set {
			_isInCinematic = value;
			if (value && Animation is not null && Animation.IsPlaying()) {
				Animation.Stop();
			}
		}
	}

	private AnimatedSprite2D? playerSprite;

	public int lightLevel = 2;
	private const int lightLevelMin = 1;
	private const int lightLevelMax = 3;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Signal]
	public delegate void ReadyToGoEventHandler();

	public bool Invulnerable { get; set; } = true;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		playerSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

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

	public void TeleportTo(Node2D target) {
		if (!target.IsInsideTree() || target.IsQueuedForDeletion()) {
			throw new InvalidOperationException("Tried teleporting to a non-existent/deleted node!");
		}

		Reparent(target.GetParent() ?? target);

		GlobalPosition = target.GlobalPosition;

		Invulnerable = false;
		EmitSignal(SignalName.ReadyToGo);
	}

	private string animationDirection = "Down";
	public override void _PhysicsProcess(double _delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		if (Input.IsActionJustPressed("dbg_kill")) {
			Die();
			return;
		}

		var delta = (float)_delta;

		if (!IsInCinematic) {
			MovePlayer(delta);
		}

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

	private void MovePlayer(float delta) {
		var direction = IsAllowedToMove
						? Input.GetVector("left", "right", "up", "down")
						: Vector2.Zero;

		var modifier = Slowed ? 0.5f : 1.0f;
		if (direction.LengthSquared() > 0.001f) {
			Velocity = direction * Speed * modifier;
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
		MoveAndSlide();

		// HACK: Cancel out wisp movement to emulate top-level movement.
		//       Can't use TopLevel=true as that breaks Y-sort.
		Wisp.GlobalPosition = wispPosition;
	}

	public void SetSpriteVisible(bool visible) {
		playerSprite!.Visible = visible;
		Shadow!.Visible = visible;

		// HACK: avoid having flipped sprite in respawn anim
		playerSprite!.FlipH = false;
	}

	public void SetMovementEnabled(bool enabled) {
		frozen = !enabled;
	}

	public void Die() {
		if (Invulnerable) {
			return;
		}

		if (IsInCinematic) {
			return;
		}
		IsInCinematic = true;

		GD.Print("I am dead.");
		FootstepsTimer?.Stop();
		this.Persistent().ResetPlayerToHub();
		LieDown();
		this.Jukebox().StopChase();
	}

	public void SetupForIntro(Node2D wispLocation) {
		SetSpriteVisible(false);
		SetMovementEnabled(false);
		WispTarget = wispLocation;
		Wisp.GlobalPosition = wispLocation.GlobalPosition;
	}

	public override void _Input(InputEvent inputEvent) {
		if (inputEvent.IsActionPressed("light_level_up")) {
			//addLightLevel(+1);
			return;
		}

		if (inputEvent.IsActionPressed("light_level_down")) {
			//addLightLevel(-1);
			return;
		}
	}

	private void AddLightLevel(int amount) {
		lightLevel += amount;
		lightLevel = Mathf.Clamp(lightLevel, lightLevelMin, lightLevelMax);

		EmitSignal(SignalName.LightLevelChanged, lightLevel);
	}

	public void LieDown() {
		if (Animation is not null) {
			if (Animation.IsPlaying()) {
				Animation.Stop();
			}
			Animation.Play("Die");
		}
	}

	public void GetUp() {
		Animation!.AnimationFinished += GetUpDone;
		Animation!.Play("GetTheFuckUp");
	}

	private void GetUpDone(StringName _) {
		Animation!.AnimationFinished -= GetUpDone;
		animationDirection = "Right";

		Animation!.Play($"Idle{animationDirection}");

		SetMovementEnabled(true);
		IsInCinematic = false;

		this.Persistent().EmitSignal(Persistent.SignalName.PlayerRespawned);
	}

	[Export]
	public AudioStreamPlayer? Noppa;
}
