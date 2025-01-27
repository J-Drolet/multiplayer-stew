using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class InGameUI : Control
{
    [Export, ExportRequired]
    public Label AmmoCount { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.InGameUI = this;
    }
}
