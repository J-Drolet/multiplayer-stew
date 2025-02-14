using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class DeathBarrier : Node
{
    [Export, ExportRequired]
    public Area3D Area;

    public override void _Ready()
    {   
        GodotErrorService.ValidateRequiredData(this);

        // server handles killing players
        if(Multiplayer.IsServer())
        {
            Area.BodyEntered += OnBodyEntered;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if(body is Character)
        {
            Character character = (Character) body;

            character.RpcId(character.GetMultiplayerAuthority(), Character.MethodName.ApplyDamage, 100000000);
        }
    }
}
