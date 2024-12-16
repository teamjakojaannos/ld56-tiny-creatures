using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiMovementState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();

		if (_pickOneOfTheseStatesWhenDoneMoving.Count == 0) {
			warnings = warnings.Append("Add one or more states to 'pick one of these when done'-list").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] private Array<NakkiAiState> _pickOneOfTheseStatesWhenDoneMoving = [];

	[Export] private float _moveTime = 3.0f;
	[Export] private float _moveTimeVariation = 0.3f;
	[Export] private float _moveToPlayerChance = 0.2f;

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
	private bool _isDoneMoving = false;

	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += () => {
			_isDoneMoving = true;
		};
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneMoving || nakki.HasReachedTarget()) {
			SelectNewState(nakki);
		}
	}

	private void SelectNewState(NakkiV2 nakki) {
		var success = TrySwitchToOneOf(nakki, _pickOneOfTheseStatesWhenDoneMoving);
		if (!success) {
			GD.PrintErr("Näkki's movement state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		var moveToPlayer = _rng.DiceRoll(_moveToPlayerChance);

		if (moveToPlayer && playerRef is Player player) {
			var relative = nakki.GetPlayerXPositionRelative(player);
			nakki.SetProgressTarget(relative);
		} else {
			var newPosition = _rng.Randf();
			nakki.SetProgressRatioTarget(newPosition);
		}

		_isDoneMoving = false;
		_timer!.WaitTime = _rng.RandomWithVariation(_moveTime, _moveTimeVariation);
		_timer!.Start();
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
		_timer!.Stop();
	}

	public override bool ShouldTickDetection() {
		return true;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) {
		StalkOrAttack(nakki, AttackState!, StalkState!);
	}
}
