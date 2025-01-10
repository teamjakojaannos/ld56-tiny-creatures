using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiAttackState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] public float _attackThreshold = 100.0f;

	[Export]
	[MustSetInEditor]
	public PackedScene WaterSplash {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_waterSplash);
		set => this.SetExportProperty(ref _waterSplash, value, notifyPropertyListChanged: true);
	}
	private PackedScene? _waterSplash;

	[Export] private float _attackTime = 1.0f;
	[Export] private float _animationSpeed = 1.0f;

	[Export]
	[MustSetInEditor]
	public NakkiUnderwaterState DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value, notifyPropertyListChanged: true);
	}
	private NakkiUnderwaterState? _diveState;


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
		nakki._hand!.Scale = player.GlobalPosition.X < nakki._nakkiEntity!.GlobalPosition.X
			? new(-1.0f, 1.0f)
			: new(1.0f, 1.0f);

		player.Slowed = true;
		nakki._attack!.GlobalPosition = player.GlobalPosition;
		nakki.PlayAttackAnimation(_attackTime, _animationSpeed);
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
			DiveState!.SetDiveTimeMult(0.25f);
		}
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }

	public override bool ShouldTickDetection() {
		return false;
	}
}
