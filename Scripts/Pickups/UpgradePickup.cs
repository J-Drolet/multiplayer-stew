using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class UpgradePickup : Pickup
{
    [Export, ExportRequired]
    public Upgrade Upgrade;

    protected override void ActivatePickup(Character character)
    {
        if(!character.Upgrades.Contains(Upgrade))
        {
            character.Rpc(Character.MethodName.AddUpgrade, (int)Upgrade);
        } 
    }
}
