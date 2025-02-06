using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class LilypadArena : Node2D {
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


	public async Task SinkLilypadsAsync(LilypadAttackStats stats, CancellationToken ct) {
		var lilypads = stats.SelectionStrategy.SelectLilypads(_lilypads);
		List<Task> tasks = [];

		foreach (var lilypad in lilypads) {
			var underwaterTime = _rng.ApplyRandomVariation(stats.UnderwaterTime, stats.UnderwaterTimeVariation);
			var sinkAnimationSpeed = _rng.ApplyRandomVariation(stats.SinkAnimationSpeed, stats.SinkAnimationSpeedVariation);
			var shakeTime = _rng.ApplyRandomVariation(stats.ShakeTime, stats.ShakeTimeVariation);
			var shakeSpeed = _rng.ApplyRandomVariation(1.0f, 0.25f);


			if (stats.RiseUpInsteadOfSink) {
				var task = lilypad.RiseUpAsync(ct);
				tasks.Add(task);
			} else {
				var task = lilypad.SinkAndRiseUpAsync(underwaterTime, sinkAnimationSpeed, shakeTime, shakeSpeed, ct);
				tasks.Add(task);
			}
		}

		await Task.WhenAny(
			ct.CancelWhenCanceled(),
			Task.WhenAll(tasks)
		);
	}

	public void ResetLilypads() {
		foreach (var lp in _lilypads) {
			lp.Reset();
		}
	}
}
