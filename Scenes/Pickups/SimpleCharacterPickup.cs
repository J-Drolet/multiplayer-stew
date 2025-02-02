using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class SimpleCharacterPickup : Label3D
{
    [Export, ExportRequired]
    public CharacterUpgradePickup upgradePickup;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        Text = upgradePickup.Upgrade.ToString();
    }
}
