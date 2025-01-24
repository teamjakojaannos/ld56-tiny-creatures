using Godot;
using System.Linq;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiSweepAttackState : NakkiAiState {
	[Export] public float Cooldown { get; set; } = 5.0f;


	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiBossIdle IdleState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_idleState);
		set => this.SetExportProperty(ref _idleState, value);
	}
	private NakkiBossIdle? _idleState;

	[Export]
	[MustSetInEditor]
	public PackedScene SweepAttackScene {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sweepAttackScene);
		set => this.SetExportProperty(ref _sweepAttackScene, value);
	}
	private PackedScene? _sweepAttackScene;

	[Export]
	[MustSetInEditor]
	public Node2D SweepAttackContainer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sweepAttackContainer);
		set => this.SetExportProperty(ref _sweepAttackContainer, value);
	}
	private Node2D? _sweepAttackContainer;

	[Export]
	public Array<Node2D> SweepAttackPositions { get; set; } = [];

	private Timer CooldownTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_cooldownTimer);
		set => this.SetExportProperty(ref _cooldownTimer, value);
	}
	private Timer? _cooldownTimer;


	private bool _attackDone;
	public bool IsOffCooldown { get; set; } = true;


	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		if (SweepAttackPositions.Count == 0) {
			warnings = warnings.Append("List of sweep attack positions is empty!").ToArray();
		}

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
		if (_attackDone) {
			nakki.CurrentState = IdleState;
		}
	}

	public override bool IsStateReady(NakkiV2 nakki) {
		return IsOffCooldown;
	}

	public override void EnterState(NakkiV2 nakki) {
		_attackDone = false;

		var sweep = SweepAttackScene.Instantiate<SweepAttack>();
		sweep.AttackDone += () => {
			_attackDone = true;
		};

		/* We need to set position before we add attack node as a child, otherwise we could
			accidentally spawn it on top of player for 1 frame (or 0.1 of a frame), causing it
			to deal damage to player
			Desired outcome: "attack global pos = marker global pos"
			=> attack global pos = attack pos + parent global pos
			   attack pos = attack global pos - parent global pos
			              = marker global pos - parent global pos
		*/
		var sweepPosition = SweepAttackPositions.PickRandom();
		var position = sweepPosition.GlobalPosition - SweepAttackContainer.GlobalPosition;
		sweep.Position = position;

		SweepAttackContainer.AddChild(sweep);
		sweep.StartAttack();
	}

	public override void ExitState(NakkiV2 nakki) {
		IsOffCooldown = false;
		CooldownTimer.Stop();
		CooldownTimer.Start(Cooldown);
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }
}

