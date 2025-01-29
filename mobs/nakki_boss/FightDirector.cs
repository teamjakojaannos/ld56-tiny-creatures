using Godot;
using System.Linq;
using System.Collections.Generic;

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

	[Export]
	[MustSetInEditor]
	public Node2D StartPosition {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_startPosition);
		set => this.SetExportProperty(ref _startPosition, value);
	}
	private Node2D? _startPosition;


	private readonly List<LilypadAttackStats> _pendingAttacks = [];
	private readonly List<Timer> _delayedAttacks = [];


	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}


	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		RegisterLilypadSignals();

		Nakki.LilypadAttackSignal += SinkLilypads;
		LilypadArena.LilypadAttackCompleted += OnLilypadAttackCompleted;

		this.Persistent().PlayerRespawned += Reset;
	}

	private void RegisterLilypadSignals() {
		foreach (var node in Nakki.GetChildren()) {
			if (node is NakkiBossStage bossStage) {
				bossStage.LilypadAttackInitiated += LilypadAttackSignalGiven;
			}
		}
	}

	private void LilypadAttackSignalGiven(LilypadAttackStats stats) {
		if (stats.Delay > 0.0f) {
			DelayedLilypadAttack(stats);
			return;
		}

		_pendingAttacks.Add(stats);
		if (stats.PlayNakkiAnimation) {
			Nakki.PlayLilypadAttackAnimation();
		} else {
			/* Normally näkki sends out a signal when it's done with the attack animation.
						animation_done_signal -> sink lilypads
				However, if we skip the animation (like with wave attack's follow-up waves),
				we can proceed directly to sinking them.
			*/
			SinkLilypads();
		}
	}

	private void SinkLilypads() {
		foreach (var stats in _pendingAttacks) {
			LilypadArena.SinkLilypads(stats);
		}

		_pendingAttacks.Clear();
	}

	private void OnLilypadAttackCompleted(int attackId) {
		if (Nakki.CurrentState is NakkiBossStage bossStage) {
			bossStage.LilypadAttackWasCompleted(attackId);
		}
	}

	private void DelayedLilypadAttack(LilypadAttackStats stats) {
		var timer = new Timer() {
			Autostart = true,
			OneShot = true,
			WaitTime = stats.Delay,
		};

		_delayedAttacks.Add(timer);

		timer.Timeout += () => {
			_delayedAttacks.Remove(timer);
			timer.QueueFree();
			LilypadArena.SinkLilypads(stats);
		};
		AddChild(timer);
	}

	private void Reset() {
		foreach (var timer in _delayedAttacks) {
			timer.Stop();
			timer.QueueFree();
		}
		_delayedAttacks.Clear();
		_pendingAttacks.Clear();
		LilypadArena.ResetLilypads();
		var relative = StartPosition.GlobalPosition - Nakki.GlobalPosition;
		Nakki.TeleportToProgress(relative.X);
	}
}
