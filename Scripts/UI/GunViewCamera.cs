using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class GunViewCamera : Camera3D
{
    [Export, ExportRequired]
    public SubViewport linkedViewport;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
    }

    public override void _Process(double delta)
    {
        if(GameManager.Players.ContainsKey(Multiplayer.GetUniqueId()))
        {
            Character character = GameManager.Players[Multiplayer.GetUniqueId()].characterNode;
            if(character != null)
            {
                GlobalTransform = character.Camera.GlobalTransform;
                Fov = character.Camera.Fov;
            }
            linkedViewport.Size = DisplayServer.WindowGetSize();
        }
    }
}
