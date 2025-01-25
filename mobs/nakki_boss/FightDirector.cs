using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class FightDirector : Node {
	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiV2 Nakki {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nakki);
		set => this.SetExportProperty(ref _nakki, value);
	}
	private NakkiV2? _nakki;

	[Export]
	[MustSetInEditor]
	public LilypadArena LilypadArena {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_lilypadArena);
		set => this.SetExportProperty(ref _lilypadArena, value);
	}
	private LilypadArena? _lilypadArena;


	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		Nakki.LilypadAttackSignal += DoLilypadAttack;
		LilypadArena.LilypadAttackCompleted += OnLilypadAttackCompleted;

		this.Persistent().PlayerRespawned += Reset;
	}

	private void DoLilypadAttack() {
		var stats = Nakki.CurrentState is HasLilypadAttack lpAttack
			? lpAttack.GetAttackStats()
			: LilypadAttackStats.Default();

		LilypadArena.SinkLilypads(stats);
	}

	private void OnLilypadAttackCompleted(int attackId) {
		if (Nakki.CurrentState is HasLilypadAttack lpAttack) {
			lpAttack.LilypadAttackWasCompleted(attackId);
		}
	}

	private void Reset() {
		LilypadArena.ResetLilypads();
	}
}
