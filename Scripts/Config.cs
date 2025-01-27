using Godot;
using Godot.Collections;
using System;

public partial class Config: Node
{
    private static ConfigFile UserSettings { get; set; }
    private static ConfigFile DefaultSettings { get; set; }

    // File paths for settings
    private static readonly string UserSettingsFilepath = "user://MultiplayerStewSettings.cfg";
    private static readonly string DefaultSettingsFilepath = "res://Config/defaults.cfg";

    // We use an instance purely so we can emit events
    public static Config Instance;

    // Signal when a setting changes
    [Signal]
    public delegate void ConfigChangedEventHandler(string section, string propertyKey, Variant propertyValue);

    public override void _EnterTree()
    {   
        Instance = this;
        
        DefaultSettings = new ConfigFile();
        DefaultSettings.Load(DefaultSettingsFilepath);

        UserSettings = new ConfigFile();
        UserSettings.Load(UserSettingsFilepath);

    }

    // Gets value from settings, reads from disk if it hasn't been read yet
    public static Variant GetValue(string section, string propertyKey)
    {
        return UserSettings.GetValue(section, propertyKey, DefaultSettings.GetValue(section, propertyKey));
    }

    public static Variant SetValue(string section, string propertyKey, Variant propertyValue)
    {
        UserSettings.SetValue(section, propertyKey, propertyValue);
        Instance.EmitSignal("ConfigChanged", section, propertyKey, propertyValue);
        return propertyValue;
    }

    public static void WriteConfig()
    {
        UserSettings.Save(UserSettingsFilepath);
    }

}
