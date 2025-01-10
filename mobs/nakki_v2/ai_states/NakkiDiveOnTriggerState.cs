using Godot;
using System;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiDiveOnTriggerState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? Array.Empty<string>())
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export]
	[MustSetInEditor]
	public NakkiUnderwaterState DiveState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_diveState);
		set => this.SetExportProperty(ref _diveState, value, notifyPropertyListChanged: true);
	}
	private NakkiUnderwaterState? _diveState;

	[Export]
	[MustSetInEditor]
	public NakkiStalkState StalkState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_stalkState);
		set => this.SetExportProperty(ref _stalkState, value, notifyPropertyListChanged: true);
	}
	private NakkiStalkState? _stalkState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value, notifyPropertyListChanged: true);
	}
	private NakkiAttackState? _attackState;


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
