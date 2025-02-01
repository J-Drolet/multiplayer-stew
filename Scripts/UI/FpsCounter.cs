using Godot;
using System;

public partial class FpsCounter : Label
{

    public override void _Ready()
    {
        Config.Instance.ConfigChanged += ShowBasedOnSettings;
        ShowBasedOnSettings("settings", "show_fps", Config.GetValue("settings", "show_fps"));
    }

    private void ShowBasedOnSettings(string section, string propertyKey, Variant propertyValue)
    {
        if(section == "settings" && propertyKey == "show_fps")
        {
            Visible = (bool)propertyValue;
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
