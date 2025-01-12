using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiAttackState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public PackedScene WaterSplash {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_waterSplash);
		set => this.SetExportProperty(ref _waterSplash, value, notifyPropertyListChanged: true);
	}
	private PackedScene? _waterSplash;

	[Export]
	[MustSetInEditor]
	public NakkiUnderwaterState DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value, notifyPropertyListChanged: true);
	}
	private NakkiUnderwaterState? _diveState;


	[ExportGroup("")]
	[Export] public float AttackThreshold { get; set; } = 100.0f;
	[Export] public float AttackTime { get; set; } = 1.0f;
	[Export] public float AnimationSpeed { get; set; } = 1.0f;

	public override void AiUpdate(NakkiV2 nakki) { }

	public override void EnterState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is not Player player) {
			return;
		}

		if (WaterSplash is not null) {
			var splash = WaterSplash.Instantiate<Node2D>();
			// we want to set position before making it visible (aka adding as a child)
			// so it isn't out of position for 1 frame
			splash.GlobalPosition = player.GlobalPosition - nakki.Position;
			nakki.AddChild(splash);
		}

		// HACK: scale instead of flipH to affect children, too
		nakki.Hand.Scale = player.GlobalPosition.X < nakki.NakkiEntity.GlobalPosition.X
			? new(-1.0f, 1.0f)
			: new(1.0f, 1.0f);

		player.Slowed = true;
		nakki.Attack.GlobalPosition = player.GlobalPosition;
		nakki.PlayAttackAnimation(AttackTime, AnimationSpeed);
	}

	public override void ExitState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		if (playerRef is Player player) {
			player.Slowed = false;
		}
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation == NakkiAnimation.Attack) {
			nakki.CurrentState = DiveState;
			DiveState.DiveTimeMultiplier = 0.25f;
		}
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }

	public override bool ShouldTickDetection() {
		return false;
	}
}
