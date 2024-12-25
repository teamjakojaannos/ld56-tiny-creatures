using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Audio;
using Jakojaannos.WisperingWoods.Gameplay.AI;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.Gameplay.Creatures.Chaser;

[Tool]
public partial class Chaser : CharacterBody2D {
	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public BehaviourTree Behaviour {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_behaviour);
		set => this.SetExportProperty(ref _behaviour, value);
	}
	private BehaviourTree? _behaviour;

	[Export]
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
		_behaviour ??= GetNode<BehaviourTree>("Behaviour");
		_footsteps ??= GetNode<RandomAudioStreamPlayer2D>("Footsteps");
		_footstepsTimer ??= _footsteps?.GetNode<Timer>("Timer");
		_attackSounds ??= GetNode<RandomAudioStreamPlayer>("AttackSounds");

		if (Engine.IsEditorHint() || this.IsMissingRequiredProperty()) {
			return;
		}
	}
}
