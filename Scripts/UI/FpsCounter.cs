using Godot;
using System;

public partial class FpsCounter : Label
{

    public override void _Ready()
    {
        //Settings.Instance.SettingChanged += ShowBasedOnSettings;
        Config.Instance.ConfigChanged += ShowBasedOnSettings;
        ShowBasedOnSettings("show_fps", Config.GetValue("settings", "show_fps"));
        //ShowBasedOnSettings("show_fps", Config.GetValue("settings", "show_fps"));
    }

    private void ShowBasedOnSettings(string propertyKey, Variant propertyValue)
    {
        if(propertyKey == "show_fps")
        {
            Visible = (bool)propertyValue;
        }
    }


    private void ShowBasedOnSettings()
    {
        GD.Print("ShowBasedOnSettings");
        Visible = (bool)Config.GetValue("settings", "show_fps");
    }


    private void ShowBasedOnSettings(string propertyKey, bool propertyValue)
    {
        if(propertyKey == "show_fps")
        {
            Visible = propertyValue;
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
