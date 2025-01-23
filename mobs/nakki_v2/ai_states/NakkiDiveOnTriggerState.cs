using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiDiveOnTriggerState : NakkiAiState {
	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiUnderwaterState DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value);
	}
	private NakkiUnderwaterState? _diveState;

	[Export]
	[MustSetInEditor]
	public NakkiStalkState StalkState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_stalkState);
		set => this.SetExportProperty(ref _stalkState, value);
	}
	private NakkiStalkState? _stalkState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value);
	}
	private NakkiAttackState? _attackState;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void ReceiveTrigger(NakkiV2 nakki) {
		nakki.CurrentState = DiveState;
	}

	public override void AiUpdate(NakkiV2 nakki) { }

	public override void EnterState(NakkiV2 nakki) { }

	public override void ExitState(NakkiV2 nakki) { }

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		StalkOrAttack(nakki, AttackState!, StalkState!);
	}
}
