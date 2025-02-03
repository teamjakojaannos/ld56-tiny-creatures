using Godot;
using System.Linq;
using System.Collections.Generic;

using Jakojaannos.WisperingWoods.Util.Editor;
using System.Threading.Tasks;
using System;
using Jakojaannos.WisperingWoods.Util;
using System.Threading;

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

	private CancellationTokenSource _lilypadAttackCancelSource = new();

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
		var ct = _lilypadAttackCancelSource.Token;
		ExecuteLilypadAttackAsync(stats, ct).FireAndForget(ct);
	}

	private async Task ExecuteLilypadAttackAsync(LilypadAttackStats stats, CancellationToken ct) {
		if (stats.Delay > 0.0f) {
			await GetTree().CreateDelay(stats.Delay);
		}

		if (stats.PlayNakkiAnimation) {
			await Nakki.PlayLilypadAttackAnimationAsync(ct).WaitOrCancel(ct);
		}

		LilypadArena.SinkLilypads(stats);
	}

	private void OnLilypadAttackCompleted(int attackId) {
		if (Nakki.CurrentState is NakkiBossStage bossStage) {
			bossStage.LilypadAttackWasCompleted(attackId);
		}
	}

	private void Reset() {
		_lilypadAttackCancelSource.Cancel();
		_lilypadAttackCancelSource = new();

		LilypadArena.ResetLilypads();
		var relative = StartPosition.GlobalPosition - Nakki.GlobalPosition;
		Nakki.TeleportToProgress(relative.X);
	}
}
