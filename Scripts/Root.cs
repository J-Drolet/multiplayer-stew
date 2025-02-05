using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;

public partial class Root : Node
{
	public static readonly string LevelsFilepath = "res://Scenes/Levels/";
	public static readonly string WeaponsFilepath = "res://Scenes/Weapons/";

	[Export, ExportRequired]
	public PackedScene ClientScene { get; set; }

	[Export, ExportRequired]
	public PackedScene ServerScene { get; set; }

	[Export, ExportRequired]
	public MultiplayerSpawner LevelSpawner { get; set; }

	public static Root Instance { get; set; }

	public override void _Ready()
	{	
		GodotErrorService.ValidateRequiredData(this);

		Instance = this;
		RegisterLevelScenes();

		bool isServer = false;
		string[] args = OS.GetCmdlineArgs();
		foreach (string arg in args)
		{
			if(arg == "--server") {
				isServer = true;;
			}
		}

		if(isServer) 
		{
			Node networkingScene = ServerScene.Instantiate();
			AddChild(networkingScene);
		}
		else 
		{
			Node networkingScene = ClientScene.Instantiate();
			AddChild(networkingScene);
		}
	}

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

	/// <summary>
	/// Automatically adds all scenes in the LevelsFilepath to be synced at the top level Multiplayer Spawner. Avoids needing to always manually add spawnable levels
	/// </summary>
	private void RegisterLevelScenes()
	{
		foreach(string filepath in GetScenesAtFilepath(LevelsFilepath)) 
		{
			LevelSpawner.AddSpawnableScene(filepath);
		}
	}
}
