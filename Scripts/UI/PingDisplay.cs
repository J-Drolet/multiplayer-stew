using Godot;
using System;

public partial class PingDisplay : Label
{

    public override void _Ready()
    {
        Config.Instance.ConfigChanged += ShowBasedOnSettings;
        ShowBasedOnSettings("settings", "show_ping", Config.GetValue("settings", "show_ping"));
    }

    private void ShowBasedOnSettings(string section, string propertyKey, Variant propertyValue)
    {
        if(section == "settings" && propertyKey == "show_ping")
        {
            Visible = (bool)propertyValue;
        }
    }

    public override void _Process(double delta)
    {
        if(Visible)
        {
            if(LevelManager.Instance != null)
            {
                Text = LevelManager.Instance.PlayerStats[Multiplayer.GetUniqueId()].ping.ToString() + "ms";
            }
            else
            {
                Text = "";
            }
        }
    }

}
