using Godot;
using System;

public partial class FpsCounter : Label
{

    public override void _Ready()
    {
        Settings.Instance.SettingChanged += ShowBasedOnSettings;
        ShowBasedOnSettings("show_fps", Settings.GetValue("show_fps"));
    }

    private void ShowBasedOnSettings(string propertyKey, string propertyValue)
    {
        if(propertyKey == "show_fps")
        {
            Visible = propertyValue == "true";
        }
    }

    public override void _Process(double delta)
    {
        if(Visible)
        {
            Text = "FPS: " + Engine.GetFramesPerSecond();
        }
    }

}
