using System.Linq;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods;

[Tool]
[GlobalClass]
public partial class WispInteractableAnimatedProp : Node2D {
	[Export]
	[MustSetInEditor]
	public AnimationPlayer AnimPlayer {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animationPlayer);
		set => this.SetExportProperty(ref _animationPlayer, value);
	}
	private AnimationPlayer? _animationPlayer;

	[Export]
	[MustSetInEditor]
	public string Animation {
		get => this.GetNotNullExportPropertyWithNullableBackingField(_animation);
		set => this.SetExportProperty(ref _animation, value);
	}
	private string? _animation;

	[Export]
	[ExportGroup("Dialogue")]
	public bool RequireDialogue = false;

	[Export]
	public string? DialogueTrigger;

	[Export]
	public bool TriggerOnlyOnce = true;

	public override string[] _GetConfigurationWarnings() {
		var warnings = base._GetConfigurationWarnings() ?? System.Array.Empty<string>();

		if (GetParentOrNull<WispInteractable>() is null) {
			warnings = warnings.Append("Parent must be a WispInteractable!").ToArray();
		}

		return warnings
			.Union(this.CheckCommonConfigurationWarnings())
			.ToArray();
	}

	public override void _Ready() {
		base._Ready();

		if (Engine.IsEditorHint()) {
			return;
		}

		var parent = GetParent<WispInteractable>();
		parent.InteractStart += () => {
			if (RequireDialogue) {
				Dialogue.Instance(this).DialogueUpdated += CheckDialogueTrigger;
			} else {
				InteractStart();
			}
		};
		parent.InteractStop += () => {
			if (RequireDialogue) {
				Dialogue.Instance(this).DialogueUpdated -= CheckDialogueTrigger;
			} else {
				AnimPlayer.Stop();
			}
		};
	}

	private void InteractStart() {
		AnimPlayer.Play(Animation);
	}

	private void InteractStop() {
		AnimPlayer.Stop();
	}

	private void CheckDialogueTrigger(string chosenOption) {
		if (!GetParent<WispInteractable>().isWispInteracting) {
			return;
		}

		if (chosenOption == DialogueTrigger) {
			InteractStart();
			if (DialogueTrigger is not null) {
				this.Persistent().State.Add(DialogueTrigger);
			}

			if (TriggerOnlyOnce) {
				GetParent<WispInteractable>().Done = true;
			}
		}
	}
}
