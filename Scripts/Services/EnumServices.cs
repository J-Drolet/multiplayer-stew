using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class EnumServices {

    public static string GetFilePath<T>(T enumValue, string parentFilepath)
    {
        List<string> scenePaths = GodotFileFindingService.GetScenesAtFilepath(parentFilepath);
        foreach(string filePath in scenePaths)
        {
            if(GodotFileFindingService.GetFileName(filePath).ToUpper() == enumValue.ToString().ToUpper())
            {
                return filePath;
            }
        }
        return null;
    }
}