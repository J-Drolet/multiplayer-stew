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
        UI.GunViewCamera = this;
    }

    public override void _Process(double delta)
    {
        linkedViewport.Size = DisplayServer.WindowGetSize();
    }
}
