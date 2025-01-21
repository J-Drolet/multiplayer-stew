using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class Root : Node
{
	[Export, ExportRequired]
	public PackedScene ClientScene { get; set; }

	[Export, ExportRequired]
	public PackedScene ServerScene { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
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
			this.AddChild(networkingScene);
		}
		else 
		{
			Node networkingScene = ClientScene.Instantiate();
			this.AddChild(networkingScene);
		}
	}
}
