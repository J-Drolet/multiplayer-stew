using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotSceneFindingService : Node
{
    public static List<string> GetScenesAtFilepath(string parentFilepath)
	{
		List<string> scenes = new();
		DirAccess dir = DirAccess.Open(parentFilepath);
		
		foreach(string filename in dir.GetFiles()) 
		{
			string filepath = parentFilepath + filename;
			string extension = GetExtension(filepath);
			if(extension == "tscn" || extension == "scn" || extension == "escn")
			{
				scenes.Add(filepath);
			}
		}

		return scenes;
	}

	private static string GetExtension(string filePath)
    {
        // Find the last dot in the file path
        int dotIndex = filePath.LastIndexOf('.');

        // Return the substring after the last dot if it exists, otherwise return an empty string
        return (dotIndex >= 0) ? filePath.Substring(dotIndex + 1) : string.Empty;
    }
}
