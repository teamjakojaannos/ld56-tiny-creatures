using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiIdleState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();

		if (PickOneOfTheseStatesWhenDoneIdling.Count == 0) {
			warnings = warnings.Append("Add one or more states to 'pick one of these when done'-list").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] public Array<NakkiAiState> PickOneOfTheseStatesWhenDoneIdling { get; set; } = [];
	[Export] public float IdleTime { get; set; } = 2.0f;
	[Export] public float IdleTimeVariation { get; set; } = 0.5f;

	[Export]
	[MustSetInEditor]
	public NakkiStalkState StalkState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_stalkState);
		set => this.SetExportProperty(ref _stalkState, value, notifyPropertyListChanged: true);
	}
	private NakkiStalkState? _stalkState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState? AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value, notifyPropertyListChanged: true);
	}
	private NakkiAttackState? _attackState;


	private Timer? _timer;
	private bool _isDoneIdling = false;
	private RandomNumberGenerator _rng = new();


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneIdling = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneIdling) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		var success = TrySwitchToOneOf(nakki, PickOneOfTheseStatesWhenDoneIdling);
		if (!success) {
			GD.PrintErr("Näkki's idle state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_isDoneIdling = false;

		_timer!.WaitTime = _rng.RandomWithVariation(IdleTime, IdleTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		_timer!.Stop();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		StalkOrAttack(nakki, AttackState!, StalkState!);
	}
}
