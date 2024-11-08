using System.Linq;

using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

public partial class NakkiUnderwaterState : NakkiAiState {
	[Export] private Array<NakkiAiState> _pickOneOfTheseStatesWhenDoneDiving = [];

	[Export] private float _underwaterTime = 5.0f;
	[Export] private float _underwaterTimeVariation = 0.3f;
	[Export] private float _emergeAtPlayerChance = 0.2f;
	[Export] private float _diveAnimationSpeed = 1.0f;
	[Export] private float _emergeAnimationSpeed = 1.0f;
	[Export] private float _diveCooldown = 10.0f;

	private Timer? _diveCooldownTimer;
	private Timer? _diveTimer;
	private bool _isDoneDiving = false;
	private bool _isEmerging = false;

	private RandomNumberGenerator _rng = new();

	private float _diveTimeMult = 1.0f;

	public override void _Ready() {
		_diveTimer = GetNode<Timer>("Timer");
		_diveTimer.Timeout += () => {
			_isDoneDiving = true;
		};

		_diveCooldownTimer = GetNode<Timer>("DiveCooldown");

		if (_pickOneOfTheseStatesWhenDoneDiving.Count == 0) {
			GD.PrintErr("Näkki's underwater state's pick-a-state-after-done-with-diving-list is empty!");
		}
	}

	public override void AiUpdate(NakkiV2 nakki) {
		if (!_isDoneDiving || _isEmerging) {
			return;
		}

		var playerRef = GetTree().GetFirstNodeInGroup("Player");
		var emergeAtPlayer = _rng.DiceRoll(_emergeAtPlayerChance);

		if (emergeAtPlayer && playerRef is Player player) {
			var relative = nakki.GetPlayerXPositionRelative(player);
			nakki.TeleportToProgress(relative);
		}

		_isEmerging = true;
		nakki.PlayEmergeFromWaterAnimation(_emergeAnimationSpeed);
	}

	private void SelectNewState(NakkiV2 nakki) {
		var possibleStates = _pickOneOfTheseStatesWhenDoneDiving.Where(s => s.IsStateReady(nakki));

		_rng.TryPickRandom(possibleStates, out var state);
		if (state != null) {
			nakki.SwitchToState(state);
		} else {
			GD.PrintErr("Näkki's underwater state failed to pick new state, resetting state to default");
			nakki.ResetStateToDefault();
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		_diveTimeMult = 1.0f;
		_isDoneDiving = false;
		_isEmerging = false;
		_diveTimer!.Stop();
		nakki.PlayDiveAnimation(_diveAnimationSpeed);
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki._detectionLevel = 0.0f;

		_diveTimer!.Stop();

		// restart timer if it was already running
		_diveCooldownTimer!.Stop();
		_diveCooldownTimer.WaitTime = _diveCooldown;
		_diveCooldownTimer.Start();
	}

	public override void NakkiAnimationFinished(NakkiV2 nakki, NakkiAnimation animation) {
		if (animation == NakkiAnimation.Dive) {
			var randomTime = _rng.RandomWithVariation(_underwaterTime, _underwaterTimeVariation);
			_diveTimer!.WaitTime = randomTime * _diveTimeMult;
			_diveTimer.Start();
			return;
		}

		if (animation == NakkiAnimation.EmergeFromWater) {
			SelectNewState(nakki);
			return;
		}
	}

	public override bool ShouldTickDetection() {
		return false;
	}

	public override void DetectionLevelChanged(NakkiV2 nakki) { }

	public void SetDiveTimeMult(float timeMult) {
		_diveTimeMult = timeMult;
	}

	public override bool IsStateReady(NakkiV2 nakki) {
		return _diveCooldownTimer!.IsStopped();
	}
}
