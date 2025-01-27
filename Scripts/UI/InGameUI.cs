using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class InGameUI : Control
{
    [Export, ExportRequired]
    public Label AmmoCount { get; set; }
    [Export, ExportRequired]
    public TextureRect HealthBar { get; set; } = new TextureRect();

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.InGameUI = this;
        HealthBar.Texture = new GradientTexture1D();
        (HealthBar.Texture as GradientTexture1D).Gradient = new();
    }

    public void SetHealthBar(float healthPercent)
    {
        float healthOffset = healthPercent;

        
        //RED POINT
        (HealthBar.Texture as GradientTexture1D).Gradient.SetOffset(0, healthOffset);
        (HealthBar.Texture as GradientTexture1D).Gradient.SetColor(0, new Color(0.85f, 0.00f, 0.00f, 1.0f));
        //CLEAR POINT
        (HealthBar.Texture as GradientTexture1D).Gradient.SetOffset(1, healthOffset + 0.0001f);
        (HealthBar.Texture as GradientTexture1D).Gradient.SetColor(1, new Color(1.0f, 1.0f, 1.0f, 0.0f));


    }
}
