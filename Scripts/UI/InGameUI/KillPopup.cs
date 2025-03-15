using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class KillPopup : Control
{
    [Export, ExportRequired]
    public Label KillLabel;

    public double TotalDisplayTime;
    public Curve OpacityCurve;
    private Timer LifeTimer = new();

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        if(Visible) // don't want to add this to prototype
        {
            LifeTimer.WaitTime = TotalDisplayTime;
            LifeTimer.Autostart = true;
            LifeTimer.Timeout += QueueFree;
            AddChild(LifeTimer);
        }
    }

    public override void _Process(double delta)
    {
        if(Visible) // don't want to add this to prototype
        {
            double percentCompleted = 1.0 - (LifeTimer.TimeLeft / TotalDisplayTime);
            Color modulateColor = Colors.White;
            modulateColor.A = OpacityCurve.Sample((float)percentCompleted);
            Modulate = modulateColor;
        }
    }
}
