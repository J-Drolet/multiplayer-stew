using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class CharacterUpgradePickup : Pickup
{
    [Export, ExportRequired]
    public CharacterUpgrade Upgrade;

    protected override void ActivatePickup(Character character)
    {
        if(!character.Upgrades.Contains(Upgrade))
        {
            character.Rpc(Character.MethodName.AddUpgrade, Variant.From(Upgrade));
        } 
    }
}
