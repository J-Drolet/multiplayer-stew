using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class EnumServices {
    public static string GetFileName(string filePath) 
    {
        return StringExtensions.GetFile(filePath).Split(".").First().ToUpper();
    }

    public static string GetFilePath<T>(T enumValue, string parentFilepath)
    {
        List<string> scenePaths = GodotSceneFindingService.GetScenesAtFilepath(parentFilepath);
        foreach(string filePath in scenePaths)
        {
            if(GetFileName(filePath).ToUpper() == enumValue.ToString().ToUpper())
            {
                return ProjectSettings.GlobalizePath(filePath);
            }
        }
        return null;
    }
}