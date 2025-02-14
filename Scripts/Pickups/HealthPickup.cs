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
        character.DamageTakenThisFrame -= HealthGained; // Entity._Process will take care of the sync
    }
}
