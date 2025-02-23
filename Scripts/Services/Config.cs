using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Config: Node
{
    private static ConfigFile UserSettings { get; set; }
    private static ConfigFile DefaultSettings { get; set; }

    // File paths for settings
    private static readonly string UserSettingsFilepath = "user://MultiplayerStewSettings.cfg";
    private static readonly string DefaultSettingsFilepath = "res://Config/constants.cfg";

    // We use an instance purely so we can emit events
    public static Config Instance;

    // Signal when a setting changes
    [Signal]
    public delegate void ConfigChangedEventHandler(string section, string propertyKey, Variant propertyValue);

    public override void _EnterTree()
    {   
        Instance = this;
        DefaultSettings = new ConfigFile();
        Error defaultError = DefaultSettings.Load(DefaultSettingsFilepath);
        if(defaultError != Error.Ok) {
            GD.PrintErr("Config._EnterTree - Failed to load default config with Status code: " + defaultError.ToString());
        }


        UserSettings = new ConfigFile();
        Error usrError = UserSettings.Load(UserSettingsFilepath);
        if(usrError != Error.Ok) {
            GD.PrintErr("Config._EnterTree - Failed to load user config with Status code: " + usrError.ToString());
        }
    }

    /// <summary>
    /// Used to go through every setting and emit the ConfigChanged signal for each setting. Specifically used for SettingsUpdater to initialize initial state of settings
    /// </summary>
    public static void EmitInitialSettings() 
    {
        foreach(string section in DefaultSettings.GetSections())
        {
            foreach(string propertyKey in DefaultSettings.GetSectionKeys(section))
            {
                Instance.EmitSignal("ConfigChanged", section, propertyKey, GetValue(section, propertyKey));
            }
        }
    }

    /// <summary>
    /// Resets the config back to the default value. Throws an error if the section and propertyKey are not found in the defaults
    /// </summary>
    /// <param name="section"></param>
    /// <param name="propertyKey"></param>
    public static void RestoreDefault(string section, string propertyKey) 
    {
        if(UserSettings.HasSectionKey(section, propertyKey)) // only has to do something if the UserSettings has the setting defined
        {
            UserSettings.EraseSectionKey(section, propertyKey);
            Instance.EmitSignal("ConfigChanged", section, propertyKey, GetValue(section, propertyKey));
        }
    }

    /// <summary>
    /// Gets a value from the config. Reads first from UserSettingsConfig and if it is not there, goes to DefaultSettingsConfig. If the requested section, propertyKey combo doesn't exist, an error is thrown
    /// </summary>
    /// <param name="section"></param>
    /// <param name="propertyKey"></param>
    /// <param name="onlyDefault"></param>
    /// <returns></returns>
    public static Variant GetValue(string section, string propertyKey, bool onlyDefault = false)
    {
        if(onlyDefault)
            return DefaultSettings.GetValue(section, propertyKey);
        else
            return UserSettings.GetValue(section, propertyKey, DefaultSettings.GetValue(section, propertyKey));
    }

    /// <summary>
    /// Sets a section,propertyKey value into the config. If the setting is reset back to default then removes it from the UserSettings for easier resetting to default. 
    /// Emits the ConfigChanged signal for the changed config
    /// </summary>
    /// <param name="section"></param>
    /// <param name="propertyKey"></param>
    /// <param name="propertyValue"></param>
    /// <returns></returns>
    public static Variant SetValue(string section, string propertyKey, Variant propertyValue)
    {
        if(propertyValue.Equals(DefaultSettings.GetValue(section, propertyKey))) // if we are back to default value, no reason to save it as a user setting
        {
            RestoreDefault(section, propertyKey);
        }
        else
        {
            UserSettings.SetValue(section, propertyKey, propertyValue);
            Instance.EmitSignal("ConfigChanged", section, propertyKey, propertyValue);
        }

        return propertyValue;
    }

    /// <summary>
    /// Saves the UserSettings ConfigFile to disk
    /// </summary>
    public static void WriteConfig()
    {
        UserSettings.Save(UserSettingsFilepath);
    }

    public static void PrintConfigFile(ConfigFile configFile)
    {
        foreach(string section in configFile.GetSections())
        {
            foreach(string sectionKey in configFile.GetSectionKeys(section)) {
                GD.Print($"{section}[{sectionKey}] = {configFile.GetValue(section, sectionKey)}");
            }
        }
    }
}
