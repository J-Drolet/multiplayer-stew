using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class HealthPickup : Pickup
{
    [Export, ExportRequired]
    public int HealthGained;

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

            {
                if (character.CurrentHealth < character.MaxHealth)
                {
                    if ((character.CurrentHealth + HealthGained) < character.MaxHealth)
                    {
                        character.CurrentHealth = (character.CurrentHealth + HealthGained);
                    }
                    if ((character.CurrentHealth + HealthGained) >= character.MaxHealth)
                    {
                        character.CurrentHealth = character.MaxHealth;
                    }
                }
            }
        }
    }
}
