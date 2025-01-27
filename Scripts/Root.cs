using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class Root : Node
{
	[Export, ExportRequired]
	public PackedScene ClientScene { get; set; }

	[Export, ExportRequired]
	public PackedScene ServerScene { get; set; }

	public static Root Instance { get; set; }

	public override void _Ready()
	{	
		Instance = this;

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
}
