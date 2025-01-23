using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class Spinner : Control
{   
    /// <summary>
    /// The speed at which the spinner spins in degrees per second
    /// </summary>
    [Export, ExportRequired]
    public float spinSpeed { get; set; } = 360f;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.Spinner = this;
    }

    public override void _Process(double delta)
    {   
        RotationDegrees += (float)(spinSpeed * delta);
    }
}
