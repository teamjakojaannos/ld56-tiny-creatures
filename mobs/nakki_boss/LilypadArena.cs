using Godot;
using Godot.Collections;
using System.Collections.Generic;

using Jakojaannos.WisperingWoods.Util;

namespace Jakojaannos.WisperingWoods;

using LilypadAttack = (int id, HashSet<BossLilypad> lilypads);

[Tool]
public partial class LilypadArena : Node2D {
	private Array<BossLilypad> _lilypads = [];
	private RandomNumberGenerator _rng = new();
	private readonly List<LilypadAttack> _ongoingAttacks = [];
	[Signal] public delegate void LilypadAttackCompletedEventHandler(int attackId);

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		foreach (var node in GetChildren()) {
			if (node is BossLilypad lilypad) {
				_lilypads.Add(lilypad);
				lilypad.LilypadEmerged += OnLilypadEmerge;
			}
		}
	}


	public void SinkLilypads(LilypadAttackStats stats) {
		LilypadAttack? attack = null;
		if (stats.AttackId is int id) {
			attack = (id, []);
		}


		var lilypads = SelectRandomLilypads(_lilypads);
		foreach (var lilypad in lilypads) {
			lilypad.UnderwaterTime = _rng.ApplyRandomVariation(stats.UnderwaterTime, stats.UnderwaterTimeVariation);
			lilypad.SinkSpeed = _rng.ApplyRandomVariation(stats.SinkSpeed, stats.SinkSpeedVariation);
			lilypad.StartSinking();

			if (attack is LilypadAttack i) {
				i.lilypads.Add(lilypad);
			}
		}

		if (attack is LilypadAttack j && j.lilypads.Count > 0) {
			_ongoingAttacks.Add(j);
		}
	}

	public void ResetLilypads() {
		_ongoingAttacks.Clear();

		foreach (var lp in _lilypads) {
			lp.ForceToSurface();
		}
	}

	private void OnLilypadEmerge(BossLilypad lp) {
		var completedAttacks = new HashSet<int>();

		foreach (var (id, lilypads) in _ongoingAttacks) {
			lilypads.Remove(lp);

			if (lilypads.Count == 0) {
				completedAttacks.Add(id);
			}
		}

		_ongoingAttacks.RemoveAll((item) => completedAttacks.Contains(item.id));

		foreach (var id in completedAttacks) {
			EmitSignal(SignalName.LilypadAttackCompleted, id);
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
