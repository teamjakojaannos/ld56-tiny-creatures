using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class NakkiEntranceTrigger : Area2D {
	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public BossLilypad LilypadToSink {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_lilypadToSink);
		set => this.SetExportProperty(ref _lilypadToSink, value);
	}
	private BossLilypad? _lilypadToSink;


	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? [];

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		BodyEntered += OnBodyEnter;
	}

	private void OnBodyEnter(Node2D node) {
		if (node is Player) {
			if (!LilypadToSink.IsUnderwaterOrAboutToSink) {
				LilypadToSink.SetSolidAndSink(sinkSpeed: 1.0f);
			}
		}
	}
}
