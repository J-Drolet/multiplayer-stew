using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;

public partial class Root : Node
{
	public static readonly string LevelsFilepath = "res://Scenes/Levels/";
	public static readonly string WeaponsFilepath = "res://Scenes/Weapons/";
	public static readonly string WeaponPickupFilepath = "res://Scenes/Pickups/Weapons/";
	public static readonly string UpgradePickupFilepath = "res://Scenes/Pickups/Upgrades/";
	public static readonly string HealthPickupFilepath = "res://Scenes/Pickups/Health/";
	public static readonly string HeadCosmeticFilepath = "res://Scenes/Cosmetics/Head/";
    public static readonly string FaceCosmeticFilepath = "res://Scenes/Cosmetics/Face/";
    public static readonly string ThumbnailFilepath = "res://Assets/Textures/Thumbnails/";
    public static readonly string ConsoleCommandFilepath = "res://Scripts/UI/Console/Commands/Implementations/";

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
		bool isManaged = false;
		string serverDataFilePath = "";
		int id = 0;
		int port = 3333; // default port
		string name = "My Server"; // default server name
        string[] args = OS.GetCmdlineArgs();
		foreach (string arg in args)
		{
			if (arg == "--server") {
				isServer = true; ;
			}
			else if(arg.Contains("--id"))
            {
                id = arg.Split('=')[1].ToInt();
            }
            else if (arg.Contains("--data-filepath"))
            {
                serverDataFilePath = arg.Split('=')[1].Trim('"');
            }
            else if (arg.Contains("--port"))
			{
				port = arg.Split('=')[1].Trim('"').ToInt();
			}
			else if (arg.Contains("--name"))
			{
				name = arg.Split('=')[1].Trim('"');
			}
			else if (arg.Contains("--managed"))
			{
				isManaged = true;
				serverDataFilePath = arg.Split('=')[1].Trim('"');
			}
		}

		if(isServer) 
		{
			Node networkingScene = ServerScene.Instantiate();
            (networkingScene as Server).ServerData.ServerID = id;
            (networkingScene as Server).ServerData.Port = port;
            (networkingScene as Server).ServerData.ServerName = name;
			(networkingScene as Server).ServerData.InLobby = true;
			(networkingScene as Server).ServerData.Players = 0;
            (networkingScene as Server).IsManaged = isManaged;
            (networkingScene as Server).ServerDataFilepath = serverDataFilePath;
            AddChild(networkingScene);
		}
		else 
		{
			Node networkingScene = ClientScene.Instantiate();
			AddChild(networkingScene);
		}
	}

	/// <summary>
	/// Automatically adds all scenes in the LevelsFilepath to be synced at the top level Multiplayer Spawner. Avoids needing to always manually add spawnable levels
	/// </summary>
	private void RegisterLevelScenes()
	{
		foreach(string filepath in GodotFileFindingService.GetScenesAtFilepath(LevelsFilepath)) 
		{
			LevelSpawner.AddSpawnableScene(filepath);
		}
	}
}
