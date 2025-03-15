using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class JsonFileService
{
    public static T ReadFromDisk<T>(string filepath)
    {
        T parsedValue;
        using (FileAccess fileAccess = FileAccess.Open(filepath, FileAccess.ModeFlags.Read))
        {
            try
            {
                parsedValue = JsonSerializer.Deserialize<T>(fileAccess.GetAsText());
            }
            catch
            {
                parsedValue = default;
            }
        }

        return parsedValue;
    }

    public static void WriteToDisk<T>(string filepath, T obj) 
    {
        using (FileAccess fileAccess = FileAccess.Open(filepath, FileAccess.ModeFlags.Write))
        {
            fileAccess.StoreString(JsonSerializer.Serialize(obj));
        }
    }
}