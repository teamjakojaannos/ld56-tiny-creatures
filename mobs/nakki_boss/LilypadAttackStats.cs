using Godot;
using Godot.Collections;

namespace Jakojaannos.WisperingWoods;

public partial class LilypadAttackStats : Resource {
	public float UnderwaterTime { get; set; } = 1.5f;
	public float UnderwaterTimeVariation { get; set; } = 0.5f;
	public float SinkSpeed { get; set; } = 1.0f;
	public float SinkSpeedVariation { get; set; } = 0.5f;
	public float ShakeTime { get; set; } = 0.75f;
	public float ShakeTimeVariation { get; set; } = 0.5f;
	public int? AttackId { get; set; } = null;
	public LilypadSelectionStrategy SelectionStrategy { get; set; } = new RandomSelection();
	public bool PlayNakkiAnimation { get; set; } = true;

	public static LilypadAttackStats Default() {
		return new();
	}


	private static int s_id = 0;
	public static int GenerateId() {
		return s_id++;
	}
}

public abstract class LilypadSelectionStrategy {
	public abstract Array<BossLilypad> SelectLilypads(Array<BossLilypad> all);
}

public class RandomSelection : LilypadSelectionStrategy {
	public override Array<BossLilypad> SelectLilypads(Array<BossLilypad> all) {
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

public class SelectByTag(string tag) : LilypadSelectionStrategy {
	public readonly string Tag = tag;

	public override Array<BossLilypad> SelectLilypads(Array<BossLilypad> all) {
		var result = new Array<BossLilypad>();

		foreach (var lp in all) {
			if (lp.Tags.Contains(Tag)) {
				result.Add(lp);
			}
		}

		return result;
	}
}
