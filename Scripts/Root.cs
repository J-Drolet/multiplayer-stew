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
