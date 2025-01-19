using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiMovementState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		if (PickOneOfTheseStatesWhenDoneMoving.Count == 0) {
			warnings = warnings.Append("Add one or more states to 'pick one of these when done'-list").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiStalkState StalkState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_stalkState);
		set => this.SetExportProperty(ref _stalkState, value);
	}
	private NakkiStalkState? _stalkState;

	[Export]
	[MustSetInEditor]
	public NakkiAttackState? AttackState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_attackState);
		set => this.SetExportProperty(ref _attackState, value);
	}
	private NakkiAttackState? _attackState;


	[ExportGroup("")]
	[Export] public Array<NakkiAiState> PickOneOfTheseStatesWhenDoneMoving { get; set; } = [];
	[Export] public float MoveTime { get; set; } = 3.0f;
	[Export] public float MoveTimeVariation { get; set; } = 0.3f;
	[Export] public float MoveToPlayerChance { get; set; } = 0.2f;

	private bool _isDoneMoving = false;
	private RandomNumberGenerator _rng = new();


	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneMoving || nakki.HasReachedTarget()) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		var success = TrySwitchToOneOf(nakki, PickOneOfTheseStatesWhenDoneMoving);
		if (!success) {
			GD.PrintErr("Näkki's movement state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		var moveToPlayer = _rng.DiceRoll(MoveToPlayerChance);

		if (moveToPlayer && playerRef is Player player) {
			var relative = nakki.GetPlayerXPositionRelative(player);
			nakki.SetProgressTarget(relative);
		} else {
			var newPosition = _rng.Randf();
			nakki.SetProgressRatioTarget(newPosition);
		}

		_isDoneMoving = false;

		var time = _rng.RandomWithVariation(MoveTime, MoveTimeVariation);
		GetTree().CreateTimer(time).Timeout += () => {
			_isDoneMoving = true;
		};
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		StalkOrAttack(nakki, AttackState!, StalkState!);
	}
}
