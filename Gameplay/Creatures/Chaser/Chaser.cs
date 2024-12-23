using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Audio;
using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
public partial class Chaser : Node2D {
	[Export]
	public BehaviourTree? Behaviour { get; set; }

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public RandomAudioStreamPlayer2D Footsteps {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footsteps);
		set => this.SetExportProperty(ref _footsteps, value);
	}
	private RandomAudioStreamPlayer2D? _footsteps;

	[Export]
	[MustSetInEditor]
	public Timer FootstepsTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_footstepsTimer);
		set => this.SetExportProperty(ref _footstepsTimer, value);
	}
	private Timer? _footstepsTimer;

	[Export]
	[MustSetInEditor]
	public RandomAudioStreamPlayer AttackSounds {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackSounds);
		set => this.SetExportProperty(ref _attackSounds, value);
	}
	private RandomAudioStreamPlayer? _attackSounds;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint()) {
			return;
		}
	}
}
