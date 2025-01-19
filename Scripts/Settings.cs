using Godot;
using System;
using System.Collections.Generic;

public partial class Settings : Node
{
    // Dictionary for storing settings and default settings
    private static Dictionary<string, string> ActiveSettings = new Dictionary<string, string>();
    private static Dictionary<string, string> DefaultSettings = new Dictionary<string, string>();

    // File paths for settings
    private static string filepath = "res://Config/settings.cfg";
    private static string defaultsFilepath = "res://Config/defaults.cfg";

    // We use an instance purely so we can emit events
    private static Settings Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    // Signal when a setting changes
    [Signal]
    public delegate void SettingChangedEventHandler(string propertyKey, string propertyValue);

    // Gets value from settings, reads from disk if it hasn't been read yet
    public static string GetValue(string propertyKey)
    {
        if (ActiveSettings.Count == 0)
        {
            ReadSettings(filepath, ActiveSettings);
        }

        if (ActiveSettings.ContainsKey(propertyKey)) // settings.cfg has the value for the property
        {
            return ActiveSettings[propertyKey];
        }
        else // we need to check the default value of the property
        {
            if (DefaultSettings.Count == 0)
            {
                ReadSettings(defaultsFilepath, DefaultSettings);
            }

            if (DefaultSettings.ContainsKey(propertyKey))
            {
                return DefaultSettings[propertyKey];
            }
            else
            {
                GD.PushError("A setting was attempted to be accessed but it does not exist in the config or the defaults");
                return string.Empty;
            }
        }
    }

    // Add value to settings, reads from disk if it hasn't been read yet
    public static void AddValue(string propertyKey, string propertyValue)
    {
        if (ActiveSettings.Count == 0)
        {
            ReadSettings(filepath, ActiveSettings);
        }

        ActiveSettings[propertyKey] = propertyValue;
        Instance.EmitSignal("SettingChanged", propertyKey, propertyValue);
    }

    // Reads settings from a file
    private static void ReadSettings(string settingsFilepath, Dictionary<string, string> settingsDict)
    {
        if (!FileAccess.FileExists(settingsFilepath))
        {
            System.IO.File.Create(settingsFilepath);
        }

        // Check if we have already attempted reading
        if (settingsDict.ContainsKey("attempted_read") && settingsDict["attempted_read"] == "true")
        {
            return;
        }

        FileAccess fileAccess = FileAccess.Open(settingsFilepath, FileAccess.ModeFlags.Read);

        while (!fileAccess.EofReached())
        {
            string curLine = fileAccess.GetLine();
            var splitSetting = curLine.Split(':');

            if (splitSetting.Length > 1)
            {
                settingsDict[splitSetting[0]] = splitSetting[1];
            }
        }

        fileAccess.Close();
        settingsDict["attempted_read"] = "true";
    }

    // Writes all the values in the settings dictionary to disk
    public static void WriteSettings()
    {
        if (ActiveSettings.Count == 0)
        {
            return;
        }

        FileAccess fileAccess = FileAccess.Open(filepath, FileAccess.ModeFlags.Write);

        foreach (var key in ActiveSettings.Keys)
        {
            fileAccess.StoreLine(key + ":" + ActiveSettings[key]);
        }

        fileAccess.Close();
    }
}