using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class HealthPickup : Pickup
{
    [Export, ExportRequired]
    public int HealthGained;

    protected override void ActivatePickup(Character character)
    {
        if (character.CurrentHealth < character.MaxHealth)
        {
            character.RpcId(character.GetMultiplayerAuthority(), Character.MethodName.SetCurrentHealth, Mathf.Clamp(character.CurrentHealth + HealthGained, 0, character.MaxHealth));
        }
    }
}
