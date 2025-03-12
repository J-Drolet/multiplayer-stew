using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotFileFindingService : Node
{
    public static List<string> GetScenesAtFilepath(string parentFilepath)
	{
		return GetFilesAtFilePath(parentFilepath, ["tscn", "scn", "escn"]);
	}

	public static List<string> GetFilesAtFilePath(string parentFilepath, string[] extensions)
	{
		HashSet<string> files = new(); // use hashset to not include duplicate filepaths
		DirAccess dir = DirAccess.Open(parentFilepath);
		
		foreach(string filename in dir.GetFiles()) 
		{
			string filepath = parentFilepath + filename;
			// When exporting the project, all scenes get a .remap and textures get a .import added that we need to remove so we can access the underlying file
			string strippedFilepath = filepath.Replace(".remap", "").Replace (".import", ""); 
			string extension = GetExtension(strippedFilepath);
			if(extensions.Contains(extension))
			{
				files.Add(strippedFilepath);
			}
		}
		return files.ToList();
	}

	public static string GetExtension(string filePath)
    {
        // Find the last dot in the file path
        int dotIndex = filePath.LastIndexOf('.');

        // Return the substring after the last dot if it exists, otherwise return an empty string
        return (dotIndex >= 0) ? filePath.Substring(dotIndex + 1) : string.Empty;
    }

	public static string GetFileName(string filePath) 
    {
        return StringExtensions.GetFile(filePath).Split(".").First();
    }
}
