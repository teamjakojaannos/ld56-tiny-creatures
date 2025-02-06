using Godot;
using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;
using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;
using Jakojaannos.WisperingWoods.Util;

using CancellationToken = System.Threading.CancellationToken;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class BossLilypad : Node2D {
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


	private Player? _player;
	public bool IsUnderwaterOrAboutToSink { get; private set; } = false;


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

		Reset();
	}

	public void Reset() {
		AnimationPlayer.Stop();
		AnimationPlayer.Play("RESET");
		IsUnderwaterOrAboutToSink = SunkenByDefault;

		if (SunkenByDefault) {
			SetSolid(true);
			Visible = false;
		} else {
			Visible = true;
			SetSolid(false);
		}
	}

	public async Task RiseUpAsync(CancellationToken ct) {
		await AnimationPlayer.PlayAsync("rise", ct);
		SetSolid(false);
	}

	public void SetSolidAndSink(float sinkSpeed) {
		SetSolid(true);
		AnimationPlayer.Play("sink", customSpeed: sinkSpeed);
	}

	public async Task SinkAndRiseUpAsync(
		float underwaterTime,
		float sinkAnimationSpeed,
		float shakeDuration,
		float shakeAnimationSpeed,
		CancellationToken ct
	) {
		IsUnderwaterOrAboutToSink = true;
		await ShakeFor(shakeDuration, shakeAnimationSpeed, ct);
		ct.ThrowIfCancellationRequested();

		await AnimationPlayer.PlayAsync("sink", ct, customSpeed: sinkAnimationSpeed);
		ct.ThrowIfCancellationRequested();
		SetSolid(true);

		await GetTree().CreateDelay(underwaterTime);
		ct.ThrowIfCancellationRequested();

		await AnimationPlayer.PlayAsync("rise", ct);
		ct.ThrowIfCancellationRequested();
		SetSolid(false);
	}

	private async Task ShakeFor(float time, float shakeAnimationSpeed, CancellationToken ct) {
		var isDone = false;
		GetTree().CreateTimer(time)
			.Timeout += () => isDone = true;

		while (!isDone) {
			if (ct.IsCancellationRequested) {
				return;
			}
			await AnimationPlayer.PlayAsync("shake", ct, customSpeed: shakeAnimationSpeed);
		}
	}

	private void DealDamage() {
		if (_player is not null) {
			// TODO:
			GD.Print("Oh no! Player died from standing on lilypad!");
		}
	}

	private void SetSolid(bool isSolid) {
		IsUnderwaterOrAboutToSink = isSolid;
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
