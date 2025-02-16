using System.Collections.Generic;
using System.Linq;
using Godot;

public static class EnumServices {
    public static string GetFileName(string filePath) 
    {
        return StringExtensions.GetFile(filePath).Split(".").First().ToUpper();
    }

    public static string GetFilePath(Weapon weapon, string parentFilepath)
    {
        List<string> weaponPickupScenes = GodotSceneFindingService.GetScenesAtFilepath(parentFilepath);
        foreach(string filePath in weaponPickupScenes)
        {
            if(GetFileName(filePath).ToUpper() == weapon.ToString().ToUpper())
            {
                return ProjectSettings.GlobalizePath(filePath);
            }
        }
        return null;
    }
}