using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class FlickeringLight : OmniLight3D
{
    [Export, ExportRequired]
    public NoiseTexture2D FlickerNoise;
    [Export, ExportRequired]
    public float FlickerIntensity = 1.0f;

    private double TimePassed = 0.0f;
    private float BaseEnergy;


    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        BaseEnergy = LightEnergy;
    }


    public override void _Process(double delta)
    {
        TimePassed += delta;
        LightEnergy = BaseEnergy + (FlickerNoise.Noise.GetNoise1D((float)TimePassed) * FlickerIntensity);
    }


}
