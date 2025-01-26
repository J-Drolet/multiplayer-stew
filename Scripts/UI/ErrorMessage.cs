using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class ErrorMessage : Control
{
    [Export, ExportRequired]
    public Label ErrorLabel { get; set; }

    public override void _Ready()
    {
        UI.ErrorMessage = this;
    }

    public void DisplayError(string errorText)
    {
        ErrorLabel.Text = errorText;
        Show();
    }

    public void OnCloseButtonPressed()
    {
        Hide();
    }
}
