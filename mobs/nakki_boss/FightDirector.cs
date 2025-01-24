using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class FightDirector : Node {
	[Export] public float CooldownBetweenAttacks { get; set; } = 2.0f;

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

	[Export]
	[MustSetInEditor]
	public NakkiSweepAttackState SweepAttack {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_sweepAttack);
		set => this.SetExportProperty(ref _sweepAttack, value);
	}
	private NakkiSweepAttackState? _sweepAttack;

	[Export]
	[MustSetInEditor]
	public Timer CooldownTimer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_cooldownTimer);
		set => this.SetExportProperty(ref _cooldownTimer, value);
	}
	private Timer? _cooldownTimer;


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

		if (AreAttacksOnCooldown()) {
			return;
		}

		var canUseLilypadAttack = LilypadAttack.IsStateReady(Nakki)
				&& LilypadArena.AreAllLilypadsUp();

		if (canUseLilypadAttack) {
			Nakki.CurrentState = LilypadAttack;
			StartCooldown();
			return;
		}

		var canUseSweepAttack = SweepAttack.IsStateReady(Nakki);
		if (canUseSweepAttack) {
			Nakki.CurrentState = SweepAttack;
			StartCooldown();
			return;
		}
	}

	private void Reset() {
		LilypadArena.ResetLilypads();
		CooldownTimer.Stop();
	}

	private bool AreAttacksOnCooldown() {
		return !CooldownTimer.IsStopped();
	}

	private void StartCooldown() {
		CooldownTimer.Stop();
		CooldownTimer.Start(CooldownBetweenAttacks);
	}
}
