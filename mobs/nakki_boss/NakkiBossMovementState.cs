using Godot;
using Godot.Collections;
using System.Linq;

using Jakojaannos.WisperingWoods.Util;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class NakkiBossMovementState : NakkiBossStage {
	[Export] public Array<string> LilypadsToSink { get; set; } = [];
	[Export] public float SinkDelay { get; set; } = 0.75f;
	[Export] public Array<string> LilypadsToRise { get; set; } = [];
	[Export] public float RiseDelay { get; set; } = 0.75f;
	[Export] public float RiseDelayVariation { get; set; } = 0.2f;

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiAiState NextState {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nextState);
		set => this.SetExportProperty(ref _nextState, value);
	}
	private NakkiAiState? _nextState;

	[Export]
	[MustSetInEditor]
	public Node2D MovementTarget {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_movementTarget);
		set => this.SetExportProperty(ref _movementTarget, value);
	}
	private Node2D? _movementTarget;

	private RandomNumberGenerator _rng = new();

	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void AiUpdate(NakkiV2 nakki) {
		if (nakki.HasReachedTarget()) {
			nakki.CurrentState = NextState;
		}
	}

	public override void EnterState(NakkiV2 nakki) {
		var relative = MovementTarget.GlobalPosition - nakki.GlobalPosition;
		nakki.SetProgressTarget(relative.X);

		for (var i = 0; i < LilypadsToSink.Count; i++) {
			var tag = LilypadsToSink[i];
			var stats = SinkStats(tag, i * SinkDelay);
			EmitSignal(NakkiBossStage.SignalName.LilypadAttackInitiated, stats);
		}

		for (var i = 0; i < LilypadsToRise.Count; i++) {
			var tag = LilypadsToRise[i];
			var delay = _rng.ApplyRandomVariation(RiseDelay, RiseDelayVariation);
			var stats = RiseUpStats(tag, i * delay);
			EmitSignal(NakkiBossStage.SignalName.LilypadAttackInitiated, stats);
		}
	}

	public override void ExitState(NakkiV2 nakki) {
		nakki.ClearMovementTarget();
	}

	public override void LilypadAttackWasCompleted(int attackId) { }
	public override void DetectionLevelChanged(NakkiV2 nakki) { }
	public override bool ShouldTickDetection() { return false; }

	private static LilypadAttackStats SinkStats(string tag, float delay) {
		return new(new SelectByTag(SelectByTag.Mode.HasAny, [tag])) {
			UnderwaterTime = 99999f,
			UnderwaterTimeVariation = 0.0f,
			SinkSpeed = 1.0f,
			SinkSpeedVariation = 0.05f,
			ShakeTime = 0.75f,
			ShakeTimeVariation = 0.05f,
			PlayNakkiAnimation = false,
			Delay = delay,
		};
	}

	private static LilypadAttackStats RiseUpStats(string tag, float delay) {
		return new(new SelectByTag(SelectByTag.Mode.HasAny, [tag])) {
			PlayNakkiAnimation = false,
			Delay = delay,
			RiseUpInsteadOfSink = true,
		};
	}
}