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
	public NakkiLilypadAttack LilypadAttack {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_lilypadAttack);
		set => this.SetExportProperty(ref _lilypadAttack, value);
	}
	private NakkiLilypadAttack? _lilypadAttack;

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

		Nakki.LilypadAttackSignal += () => {
			LilypadArena.SinkLilypads();
		};

		this.Persistent().PlayerRespawned += Reset;
	}

	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) {
			return;
		}

		var isIdling = Nakki.CurrentState is NakkiBossIdle;
		if (!isIdling) {
			return;
		}

		var canUseLilypadAttack = LilypadAttack.IsStateReady(Nakki)
				&& LilypadArena.AreAllLilypadsUp();

		if (canUseLilypadAttack) {
			Nakki.CurrentState = LilypadAttack;
			return;
		}
	}

	private void Reset() {
		LilypadArena.ResetLilypads();
	}
}
