using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class SimplePickup : Label3D
{
    [Export, ExportRequired]
    public UpgradePickup upgradePickup;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        Text = upgradePickup.Upgrade.ToString();
    }
}
