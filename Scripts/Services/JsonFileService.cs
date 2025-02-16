using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class JsonFileService
{
    public static T ReadFromDisk<T>(string filepath)
    {
        string jsonFilePath = ProjectSettings.GlobalizePath(filepath);
        T parsedValue;
        try
        {
            parsedValue = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonFilePath));
        }
        catch
        {
            parsedValue = default;
        }

        return parsedValue;
    }

    public static void WriteToDisk<T>(string filepath, T obj) 
    {
        string jsonFilePath = ProjectSettings.GlobalizePath(filepath);
        File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(obj));
    }
}