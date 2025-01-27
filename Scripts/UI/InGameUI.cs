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
    }

    public void SetHealthBar(float healthPercent)
    {
        GradientTexture1D healthGradient = new();
        healthGradient.Gradient = new();

        //RED POINT
        healthGradient.Gradient.AddPoint(healthPercent, new Color(0.85f, 0.00f, 0.00f, 1.0f));
        //CLEAR POINT
        healthGradient.Gradient.AddPoint(healthPercent + 0.0001f, new Color(1.0f, 1.0f, 1.0f, 0.0f));
        HealthBar.Texture = healthGradient;
    }
}
