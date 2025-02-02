using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class CharacterUpgradePickup : Pickup
{
    [Export, ExportRequired]
    public CharacterUpgrade Upgrade;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        base._Ready();
    }

    protected override void ActivatePickup(Node3D body)
    {
        if (body is Character)
        {
            Character character = body as Character;

            if(!character.Upgrades.Contains(Upgrade))
            {
                character.Upgrades.Add(Upgrade);

                if (DestroyOnPickup)
                {
                    QueueFree();
                }
            }
        }
    }
}
