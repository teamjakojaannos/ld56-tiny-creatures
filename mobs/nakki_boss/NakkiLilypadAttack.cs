using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiLilypadAttack : NakkiAiState {
	[Export]
	public float Cooldown { get; set; } = 10.0f;


	[Export]
	[MustSetInEditor]
	public NakkiBossIdle IdleState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_idleState);
		set => this.SetExportProperty(ref _idleState, value);
	}
	private NakkiBossIdle? _idleState;

	private Timer CooldownTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_cooldownTimer);
		set => this.SetExportProperty(ref _cooldownTimer, value);
	}
	private Timer? _cooldownTimer;


	private bool _animationDone;
	public bool IsOffCooldown { get; set; } = true;


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

		CooldownTimer = new Timer {
			Autostart = false,
			OneShot = true,
		};
		CooldownTimer.Timeout += () => {
			IsOffCooldown = true;
		};
		AddChild(CooldownTimer);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_animationDone) {
			nakki.CurrentState = IdleState;
		}
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation != NakkiAnimation.LilypadAttack) {
			return;
		}

		_animationDone = true;
	}

	public override bool IsStateReady(NakkiV2 nakki) {
		return IsOffCooldown;
	}

	public override void EnterState(NakkiV2 nakki) {
		_animationDone = false;
		nakki.PlayLilypadAttackAnimation();
	}

	public override void ExitState(NakkiV2 nakki) {
		IsOffCooldown = false;
		CooldownTimer.Stop();
		CooldownTimer.Start(Cooldown);
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }
}
