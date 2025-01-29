using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class BossLilypad : Node2D {
	[Export] public float UnderwaterTime { get; set; } = 1.5f;
	[Export] public float SinkSpeed { get; set; } = 1.0f;
	[Export] public float ShakeAnimationSpeed { get; set; } = 1.0f;
	[Export] public Array<string> Tags { get; set; } = [];

	[Export]
	public bool SunkenByDefault {
		get => _sunkenByDefault;
		set {
			_sunkenByDefault = value;
			Visible = !SunkenByDefault;
		}
	}
	private bool _sunkenByDefault = false;



	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public AnimationPlayer AnimationPlayer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animationPlayer);
		set => this.SetExportProperty(ref _animationPlayer, value);
	}
	private AnimationPlayer? _animationPlayer;

	[Export]
	[MustSetInEditor]
	public CollisionShape2D Blocker {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_blocker);
		set => this.SetExportProperty(ref _blocker, value);
	}
	private CollisionShape2D? _blocker;

	private Timer UnderwaterTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_underwaterTimer);
		set => this.SetExportProperty(ref _underwaterTimer, value);
	}
	private Timer? _underwaterTimer;

	private Timer ShakeTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_shakeTimer);
		set => this.SetExportProperty(ref _shakeTimer, value);
	}
	private Timer? _shakeTimer;


	private Player? _player;
	private bool _isDoneShaking;
	[Signal] public delegate void LilypadEmergedEventHandler(BossLilypad lilypad);


	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		var playerDetector = GetNode<Area2D>("PlayerDetector");
		playerDetector.BodyEntered += OnBodyEnter;
		playerDetector.BodyExited += OnBodyExit;

		UnderwaterTimer = new Timer {
			Autostart = false,
			OneShot = true,
		};
		UnderwaterTimer.Timeout += RiseUp;
		AddChild(UnderwaterTimer);

		ShakeTimer = new Timer {
			Autostart = false,
			OneShot = true,
		};
		ShakeTimer.Timeout += () => {
			_isDoneShaking = true;
		};
		AddChild(ShakeTimer);

		AnimationPlayer.AnimationFinished += (name) => {
			if (name == "sink") {
				SinkAnimationDone();
			} else if (name == "rise") {
				RiseAnimationDone();
			} else if (name == "shake") {
				ShakeAnimationDone();
			}
		};

		Reset();
	}

	public void StartSinking(
		float underwaterTime,
		float sinkSpeed,
		float shakeTime,
		float shakeAnimationSpeed
	) {
		UnderwaterTime = underwaterTime;
		SinkSpeed = sinkSpeed;
		ShakeAnimationSpeed = shakeAnimationSpeed;

		_isDoneShaking = false;
		ShakeTimer.Start(shakeTime);
		AnimationPlayer.Play("shake", customSpeed: ShakeAnimationSpeed);
	}

	private void ShakeAnimationDone() {
		if (_isDoneShaking) {
			AnimationPlayer.Play("sink", customSpeed: SinkSpeed);
			return;
		}

		AnimationPlayer.Play("shake", customSpeed: ShakeAnimationSpeed);
	}

	private void SinkAnimationDone() {
		SetSolid(true);
		UnderwaterTimer.Start(UnderwaterTime);
	}

	public void RiseUp() {
		AnimationPlayer.Play("rise");
	}

	private void RiseAnimationDone() {
		SetSolid(false);
		EmitSignal(SignalName.LilypadEmerged, this);
	}

	public void Reset() {
		UnderwaterTimer.Stop();
		AnimationPlayer.Stop();
		ShakeTimer.Stop();

		if (SunkenByDefault) {
			SetSolid(true);
			Visible = false;
		} else {
			Visible = true;
			SetSolid(false);
		}
	}

	private void DealDamage() {
		if (_player is not null) {
			// TODO:
			GD.Print("Oh no! Player died from standing on lilypad!");
		}
	}

	private void SetSolid(bool isSolid) {
		Blocker.SetDeferred(CollisionShape2D.PropertyName.Disabled, !isSolid);
	}

	private void OnBodyEnter(Node2D node) {
		if (node is Player player) {
			_player = player;
		}
	}

	private void OnBodyExit(Node2D node) {
		if (node is Player) {
			_player = null;
		}
	}
}
