using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiUnderwaterState : NakkiAiState {
	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		if (PickOneOfTheseStatesWhenDoneDiving.Count == 0) {
			warnings = warnings.Append("Add one or more states to 'pick one of these when done'-list").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	[Export] public Array<NakkiAiState> PickOneOfTheseStatesWhenDoneDiving { get; set; } = [];
	[Export] public float UnderwaterTime { get; set; } = 5.0f;
	[Export] public float UnderwaterTimeVariation { get; set; } = 0.3f;
	[Export] public float EmergeAtPlayerChance { get; set; } = 0.2f;
	[Export] public float DiveAnimationSpeed { get; set; } = 1.0f;
	[Export] public float EmergeAnimationSpeed { get; set; } = 1.0f;
	[Export] public float DiveCooldown { get; set; } = 10.0f;
	[Export] public float MaxEmergeDistance { get; set; } = 100.0f;


	private bool _isDoneDiving = false;
	private bool _isEmerging = false;
	private RandomNumberGenerator _rng = new();
	public float DiveTimeMultiplier { get; set; } = 1.0f;

	private Timer? _diveCooldownTimer;
	private bool _isReadyToDive = true;


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		_diveCooldownTimer = new Timer {
			Autostart = false,
			OneShot = true
		};
		_diveCooldownTimer.Timeout += () => {
			_isReadyToDive = true;
		};
		AddChild(_diveCooldownTimer);
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (_isDoneDiving && !_isEmerging) {
			EmergeFromWater(nakki);
		}
	}

	private void EmergeFromWater(NakkiV2 nakki) {
		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		var pathLength = nakki.PathLength;

		var emergeTo = (_rng.DiceRoll(EmergeAtPlayerChance) && playerRef is Player player)
				? nakki.GetPlayerXPositionRelative(player)
				: _rng.RandfRange(0.0f, pathLength);

		var rand = _rng.RandfRange(-MaxEmergeDistance, MaxEmergeDistance);
		var emergeFrom = emergeTo + rand;
		emergeFrom = Mathf.Clamp(emergeFrom, 0f, pathLength);

		// teleport first so it doesn't clear the move target
		nakki.TeleportToProgress(emergeFrom);
		nakki.SetProgressTarget(emergeTo);

		_isEmerging = true;
		nakki.PlayEmergeFromWaterAnimation(EmergeAnimationSpeed);
	}

	private void SelectNewState(NakkiV2 nakki) {
		var success = TrySwitchToOneOf(nakki, PickOneOfTheseStatesWhenDoneDiving);
		if (!success) {
			GD.PrintErr("Näkki's underwater state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		DiveTimeMultiplier = 1.0f;
		_isDoneDiving = false;
		_isEmerging = false;
		_isReadyToDive = false;
		nakki.PlayDiveAnimation(DiveAnimationSpeed);
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.DetectionLevel = 0.0f;

		// näkki will be forced into dive mode when missing an attack
		// we need to reset timer if that happens
		_diveCooldownTimer!.Stop();
		_diveCooldownTimer.Start(DiveCooldown);
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation == NakkiAnimation.Dive) {
			var randomTime = _rng.RandomWithVariation(UnderwaterTime, UnderwaterTimeVariation);
			var time = randomTime * DiveTimeMultiplier;
			GetTree().CreateTimer(time).Timeout += () => {
				_isDoneDiving = true;
			};
			return;
		}

		if (animation == NakkiAnimation.EmergeFromWater) {
			SelectNewState(nakki);
			return;
		}
	}

	public override bool ShouldTickDetection() {
		return _isEmerging;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }

	public override bool IsStateReady(NakkiV2 nakki) {
		return _isReadyToDive;
	}
}
