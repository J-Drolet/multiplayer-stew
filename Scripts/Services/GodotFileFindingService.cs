using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotFileFindingService : Node
{
	public static string[] ImportedFileExtensions = ["png"];

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
			string removedRemapFile = filepath.Replace (".remap", ""); // export adds unnecessary .remap extension
			string extension = GetExtension(removedRemapFile);
			if(extensions.Contains(extension))
			{
				files.Add(removedRemapFile);
			}
			else // check if no import is present
			{
				foreach(string targetedFileExtension in extensions)
				{
					if(ImportedFileExtensions.Contains(targetedFileExtension)) // if this is a filetype that is accessed by an import
					{
						string removedImportFile = filepath.Replace (".import", "");
						extension = GetExtension(removedImportFile);
						if(targetedFileExtension == extension)
						{
							files.Add(removedImportFile);
						}
					}
				}
				
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
