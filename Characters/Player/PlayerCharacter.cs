using System;
using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Audio;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Characters.Player;

[Tool]
[GlobalClass]
public partial class PlayerCharacter : CharacterBody2D {
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

	public bool IsAllowedToMove => true;//!Dialogue.Instance(this).Visible && !frozen && !IsInCinematic;

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

	public bool SpriteVisible {
		get => playerSprite?.Visible ?? false;
		set {
			if (Shadow is not null) Shadow.Visible = value;

			if (playerSprite is not null) {
				playerSprite.Visible = value;

				// HACK: avoid having flipped sprite in respawn anim
				playerSprite!.FlipH = false;
			}

		}
	}

	public bool MovementEnabled {
		get => !frozen;
		set => frozen = !value;
	}

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Signal]
	public delegate void ReadyToGoEventHandler();

	[Signal]
	public delegate void TeleportedEventHandler();

	public bool Invulnerable { get; set; } = true;

	private uint? _collisionLayer;
	private uint? _collisionMask;

	public override void _EnterTree() {
		_collisionLayer ??= CollisionLayer;
		_collisionMask ??= CollisionMask;

		if (!Engine.IsEditorHint()) {
			DisableCollision();
		}
	}

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

	internal void RestoreCollision() {
		CollisionLayer = _collisionLayer!.Value;
		CollisionMask = _collisionMask!.Value;
	}

	private void DisableCollision() {
		CollisionLayer = 0;
		CollisionMask = 0;
	}

	private string animationDirection = "Down";
	public override void _PhysicsProcess(double _delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var delta = (float)_delta;

		if (!IsInCinematic) {
			MovePlayer(delta);
		}
	}

	public Vector2 InputDirection { get; private set; }

	private void MovePlayer(float delta) {
		InputDirection = IsAllowedToMove
						? Input.GetVector("left", "right", "up", "down")
						: Vector2.Zero;

		var modifier = Slowed ? 0.5f : 1.0f;
		if (InputDirection.LengthSquared() > 0.001f) {
			Velocity = InputDirection * Speed * modifier;
			animationDirection = InputDirection.X < 0.0
				? "Left"
				: InputDirection.X > 0.0
				? "Right"
				: InputDirection.Y < 0.0
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

		MoveAndSlide();
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
		//this.Persistent().ResetPlayerToHub();
		LieDown();
		this.Jukebox().StopChase();
	}

	[Obsolete("Separation of concerns: should live in intro instead")]
	public void SetupForIntro() {
		SpriteVisible = false;
		MovementEnabled = false;
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

		MovementEnabled = true;
		IsInCinematic = false;

		//this.Persistent().EmitSignal(Persistent.SignalName.PlayerRespawned);
	}

	[Export]
	public AudioStreamPlayer? Noppa;
}
