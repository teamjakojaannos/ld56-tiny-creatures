using Godot;
using Godot.Collections;

using System.Collections.Generic;
using System.Linq;

namespace Jakojaannos.WisperingWoods;

public partial class LilypadAttackStats(LilypadSelectionStrategy selectionStrategy) : Resource {
	public float UnderwaterTime { get; set; } = 1.5f;
	public float UnderwaterTimeVariation { get; set; } = 0.5f;
	public float SinkSpeed { get; set; } = 1.0f;
	public float SinkSpeedVariation { get; set; } = 0.5f;
	public float ShakeTime { get; set; } = 0.75f;
	public float ShakeTimeVariation { get; set; } = 0.5f;
	public int AttackId { get; set; } = GenerateId();
	public LilypadSelectionStrategy SelectionStrategy { get; set; } = selectionStrategy;
	public bool PlayNakkiAnimation { get; set; } = true;


	private static int s_id = 0;
	public static int GenerateId() {
		return s_id++;
	}
}

public abstract class LilypadSelectionStrategy {
	public abstract Array<BossLilypad> SelectLilypads(Array<BossLilypad> all);
}

public class RandomSelection(int amount, string tag) : LilypadSelectionStrategy {
	public readonly int Amount = amount;
	public readonly string Tag = tag;

	public override Array<BossLilypad> SelectLilypads(Array<BossLilypad> all) {
		var tagged = new Array<BossLilypad>(all.Where(lp => lp.Tags.Contains(Tag)));
		var result = new Array<BossLilypad>();

		for (var i = 0; i < Amount; i++) {
			if (tagged.Count == 0) {
				break;
			}

			var item = tagged.PickRandom();
			result.Add(item);
			tagged.Remove(item);
		}

		return result;
	}
}

public class SelectByTag(SelectByTag.Mode mode, IEnumerable<string> tags) : LilypadSelectionStrategy {
	public readonly Mode SelectionMode = mode;
	public readonly HashSet<string> Tags = new(tags);

	public override Array<BossLilypad> SelectLilypads(Array<BossLilypad> all) {
		return new(all.Where(Passes));
	}

	private bool Passes(BossLilypad lp) {
		if (SelectionMode == Mode.HasAny) {
			return HasAny(lp);
		} else {
			return HasAll(lp);
		}
	}

	private bool HasAny(BossLilypad lp) {
		foreach (var tag in Tags) {
			if (lp.Tags.Contains(tag)) {
				return true;
			}
		}
		return false;
	}

	private bool HasAll(BossLilypad lp) {
		foreach (var tag in Tags) {
			if (!lp.Tags.Contains(tag)) {
				return false;
			}
		}
		return true;
	}


	public enum Mode {
		HasAll,
		HasAny,
	}
}
