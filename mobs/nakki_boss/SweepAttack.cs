using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class SweepAttack : Node2D {
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

	[Signal] public delegate void AttackDoneEventHandler();


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
			if (name == "attack") {
				EmitSignal(SignalName.AttackDone);
				QueueFree();
			}
		};
	}

	public void StartAttack() {
		AnimationPlayer.Play("attack");
	}

	private void OnBodyEnter(Node2D node) {
		if (node is Player) {
			GD.Print("Player was hit by Näkki sweep attack!");
		}
	}
}
