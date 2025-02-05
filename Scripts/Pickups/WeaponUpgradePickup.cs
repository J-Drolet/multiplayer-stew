using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class WeaponUpgradePickup : Pickup
{
    [Export, ExportRequired]
    public WeaponUpgrade Upgrade;

    protected override void ActivatePickup(Character character)
    {
        if(character.EquippedWeapon != null && !character.EquippedWeapon.Upgrades.Contains(Upgrade))
        {
            character.EquippedWeapon.Rpc(UpgradableWeapon.MethodName.AddUpgrade, Variant.From(Upgrade));
        }
    }
}
