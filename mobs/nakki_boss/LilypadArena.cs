using Godot;
using Godot.Collections;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class LilypadArena : Node2D {
	[Export] public float UnderwaterTime { get; set; } = 1.5f;
	[Export] public float UnderwaterTimeVariation { get; set; } = 0.5f;
	[Export] public float SinkSpeed { get; set; } = 1.0f;
	[Export] public float SinkSpeedVariation { get; set; } = 0.5f;

	private Array<BossLilypad> _lilypads = [];
	private RandomNumberGenerator _rng = new();

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		foreach (var node in GetChildren()) {
			if (node is BossLilypad lilypad) {
				_lilypads.Add(lilypad);
			}
		}
	}

	public bool AreAllLilypadsUp() {
		foreach (var lilypad in _lilypads) {
			if (lilypad.IsUnderwater) {
				return false;
			}
		}

		return true;
	}

	public void SinkLilypads() {
		var lilypads = SelectRandomLilypads(_lilypads);
		foreach (var lilypad in lilypads) {
			lilypad.UnderwaterTime = _rng.ApplyRandomVariation(UnderwaterTime, UnderwaterTimeVariation);
			lilypad.SinkSpeed = _rng.ApplyRandomVariation(SinkSpeed, SinkSpeedVariation);
			lilypad.StartSinking();
		}
	}

	public void ResetLilypads() {
		foreach (var lp in _lilypads) {
			lp.ForceToSurface();
		}
	}

	private static Array<BossLilypad> SelectRandomLilypads(Array<BossLilypad> all) {
		var maxAmount = 5;
		var list = new Array<BossLilypad>(all);
		var result = new Array<BossLilypad>();

		for (var i = 0; i < maxAmount; i++) {
			if (list.Count == 0) {
				break;
			}

			var item = list.PickRandom();
			result.Add(item);
			list.Remove(item);
		}

		return result;
	}
}
