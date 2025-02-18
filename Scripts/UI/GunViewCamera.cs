using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class GunViewCamera : Camera3D
{
    [Export, ExportRequired]
    public SubViewport linkedViewport;

    public bool active = false;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.GunViewCamera = this;
    }

    public override void _Process(double delta)
    {
        if(!active) return;

        if(LevelManager.Instance.LevelPeerInfo.ContainsKey(Multiplayer.GetUniqueId()))
        {
            Character character = LevelManager.Instance.LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode;
            if(character != null)
            {
                GlobalTransform = character.Camera.GlobalTransform;
                Fov = character.Camera.Fov;
            }
            linkedViewport.Size = DisplayServer.WindowGetSize();
        }
    }
}
