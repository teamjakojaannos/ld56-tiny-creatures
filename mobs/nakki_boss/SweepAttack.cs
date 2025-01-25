using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class SweepAttack : Node2D {
	[Export] public float Speed { get; set; } = 50.0f;


	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public Area2D PlayerDetector {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_playerDetector);
		set => this.SetExportProperty(ref _playerDetector, value);
	}
	private Area2D? _playerDetector;

	[Export]
	[MustSetInEditor]
	public AnimationPlayer AnimationPlayer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animationPlayer);
		set => this.SetExportProperty(ref _animationPlayer, value);
	}
	private AnimationPlayer? _animationPlayer;

	[Export]
	[MustSetInEditor]
	public PathFollow2D PathFollow {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_pathFollow);
		set => this.SetExportProperty(ref _pathFollow, value);
	}
	private PathFollow2D? _pathFollow;

	[Signal] public delegate void AttackDoneEventHandler();


	private bool _isMoving = false;


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

		PlayerDetector.BodyEntered += OnBodyEnter;
		AnimationPlayer.AnimationFinished += (name) => {
			if (name == "disappear") {
				EmitSignal(SignalName.AttackDone);
				QueueFree();
			}
		};
	}

	public override void _Process(double delta) {
		if (!_isMoving) {
			return;
		}

		PathFollow.Progress += Speed * (float)delta;

		if (PathFollow.ProgressRatio >= 1.0f) {
			Disappear();
		}
	}

	public void StartAttack() {
		AnimationPlayer.Play("start_attack");
	}

	private void OnBodyEnter(Node2D node) {
		if (node is Player) {
			GD.Print("Player was hit by Näkki sweep attack!");
		}
	}

	private void StartMoving() {
		_isMoving = true;
	}

	private void Disappear() {
		_isMoving = false;
		AnimationPlayer.Play("disappear");
	}
}
