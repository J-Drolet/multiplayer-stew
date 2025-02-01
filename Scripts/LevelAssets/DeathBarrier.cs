using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class DeathBarrier : Node
{
    [Export, ExportRequired]
    public Area3D Area;

    public override void _Ready()
    {   
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

            character.RpcId(character.GetMultiplayerAuthority(), Character.MethodName.SetCurrentHealth, 0);
        }
    }
}
