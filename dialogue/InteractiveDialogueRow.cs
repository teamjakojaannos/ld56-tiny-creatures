using System.Collections.Generic;
using Godot;

public partial class InteractiveDialogueRow : DialogueRow {
    [Export]
    public Color DialogueColor = Colors.DarkGray;

    [Export]
    public Color SelectedDialogueColor = Colors.Wheat;

    [Export]
    public Color InactiveDialogueColor = Colors.Black;

    public int OptionCount => GetNode("Options").GetChildCount();

    public int HighlightedOption { get; private set; } = 0;

    protected override bool AutoplayAudio() {
        return false;
    }

    public override bool IsReady => true;

    public void SetupOptions(PackedScene template, IEnumerable<string> optionLines) {
        var options = GetNode("Options");

        var idx = 0;
        foreach (var line in optionLines) {
            var option = template.Instantiate<DialogueOption>();
            option.Text = line;
            option.OptionIndex = idx++;
            option.Row = this;
            options.AddChild(option);
        }

        SetupNumbers();
        HighlightOption(0);
    }

    public void SetupNumbers() {
        var numberLabel = GetNode<Label>("Numbers");
        numberLabel.Text = "";
        for (var i = 0; i < OptionCount; i++) {
            numberLabel.Text += $"{i + 1}:";

            if (i != OptionCount - 1) {
                numberLabel.Text += "\n";
            }
        }
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
        HighlightedOption = option;

        var selectionLabel = GetNode<Label>("Selection");
        selectionLabel.Text = "";
        for (int i = 0; i < OptionCount; i++) {
            var optionLabel = GetNode("Options").GetChild<Label>(i);

            if (i == option) {
                selectionLabel.Text += ">";
                optionLabel.LabelSettings.FontColor = SelectedDialogueColor;
            } else {
                selectionLabel.Text += " ";
                optionLabel.LabelSettings.FontColor = DialogueColor;
            }

            if (i != OptionCount - 1) {
                selectionLabel.Text += "\n";
            }
        }
    }

    public void SelectOption(int option) {
        HighlightedOption = option;

        var selectionLabel = GetNode<Label>("Selection");
        selectionLabel.Text = "";
        for (int i = 0; i < OptionCount; i++) {
            var optionLabel = GetNode("Options").GetChild<Label>(i);

            if (i == option) {
                selectionLabel.Text += " ";
                optionLabel.LabelSettings.FontColor = SelectedDialogueColor;
            } else {
                selectionLabel.Text += " ";
                optionLabel.LabelSettings.FontColor = InactiveDialogueColor;
            }

            if (i != OptionCount - 1) {
                selectionLabel.Text += "\n";
            }
        }
    }
}
