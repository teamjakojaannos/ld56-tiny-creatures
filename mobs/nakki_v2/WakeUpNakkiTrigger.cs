using Godot;
using System.Linq;

using Jakojaannos.WisperingWoods.Characters.Player;
using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
public partial class WakeUpNakkiTrigger : Area2D {

	[Export]
	[ExportGroup("Prewire")]
	[MustSetInEditor]
	public NakkiV2 NakkiToTrigger {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_nakkiToTrigger);
		set => this.SetExportProperty(ref _nakkiToTrigger, value);
	}
	private NakkiV2? _nakkiToTrigger;

	public override string[] _GetConfigurationWarnings() {
		return (base._GetConfigurationWarnings() ?? [])
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			return;
		}

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D node) {
		if (node is PlayerCharacter) {
			NakkiToTrigger.PlayerEnteredTrigger();
		}
	}
}
