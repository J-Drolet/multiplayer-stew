using System;
using Godot;

public partial class SettingsUpdater: Node 
{
    public override void _Ready()
    {
        Config.Instance.ConfigChanged += SettingsUpdated;

        SettingsUpdated("settings", "display_mode", Config.GetValue("settings", "display_mode"));
    }

    private void SettingsUpdated(string section, string propertyKey, Variant propertyValue)
    {
        switch(section) {
            case "settings":
                switch(propertyKey) {
                    case "display_mode":
                        switch((int) propertyValue) {
                            case 0:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                                break;
                            case 1:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                                break;
                            case 2:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                                break;
                        }
                        break;
                    case "music_volume":
                        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb((float) propertyValue));
                        break;
                    case "sfx_volume":
                        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb((float) propertyValue));
                        break;
                }

                break;
        }
        
    }
}