using System.Collections.Generic;
using Godot;

public partial class InteractiveDialogueRow : DialogueRow {
    [Export]
    public Color DialogueColor = Colors.DarkGray;

    [Export]
    public Color SelectedDialogueColor = Colors.Wheat;

    [Export]
    public Color InactiveDialogueColor = Colors.Black;

    [Export]
    public Control? Options;

    public int OptionCount => Options?.GetChildCount() ?? 0;

    public int HighlightedOption { get; private set; } = 0;

    protected override bool AutoplayAudio() {
        return false;
    }

    public override bool IsReady => true;

    public void SetupOptions(PackedScene template, IEnumerable<string> optionLines) {
        var idx = 0;
        foreach (var line in optionLines) {
            var option = template.Instantiate<DialogueOption>();
            option.Text = line;
            option.OptionIndex = idx++;
            option.Row = this;
            Options?.AddChild(option);
        }

        HighlightOption(0);
    }

    public void ClearOptions() {
        if (OptionCount == 0) {
            return;
        }

        var options = GetNode("Options");
        foreach (var child in options.GetChildren()) {
            options.RemoveChild(child);
            child.QueueFree();
        }
    }

    public void HighlightOption(int option) {
        if (Options is null) {
            return;
        }

        HighlightedOption = option;

        for (int i = 0; i < OptionCount; i++) {
            var dialogueOption = Options.GetChild<DialogueOption>(i);

            if (i == option) {
                dialogueOption.Select();
                dialogueOption.LabelColor = SelectedDialogueColor;
            } else {
                dialogueOption.Deselect();
                dialogueOption.LabelColor = DialogueColor;
            }
        }
    }

    public void SelectOption(int option) {
        if (Options is null) {
            return;
        }

        HighlightedOption = option;

        for (int i = 0; i < OptionCount; i++) {
            var dialogueOption = GetNode("Options").GetChild<DialogueOption>(i);

            dialogueOption.Deactivate();
            if (i == option) {
                dialogueOption.LabelSettings!.FontColor = SelectedDialogueColor;
            } else {
                dialogueOption.LabelSettings!.FontColor = InactiveDialogueColor;
            }
        }
    }
}
