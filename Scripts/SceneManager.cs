using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SceneManager : Node
{   
	[Export, ExportRequired]
	public PackedScene PlayerScene { get; set; }

	public List<Node3D> SpawnPoints;

	public override void _Ready() 
	{   
		GodotErrorService.ValidateRequiredData(this);

		if(!Multiplayer.IsServer()) {
			UI.InGameUI.Show();
		}

		SpawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoint").Select(s => s as Node3D).ToList();

		// spawning characters on spawn points (ran on every peer)
		List<long> players = GameManager.Players.Keys.OrderBy(p => p).ToList();

		for (int i = 0; i < players.Count; i++)
		{   
			if(players[i] == 1) {
				continue; // skip the server
			}
			GameManager.PlayerInfo playerInfo = GameManager.Players[players[i]];

			Character currentPlayer = (Character) PlayerScene.Instantiate();
			currentPlayer.Name = players[i].ToString();
			AddChild(currentPlayer);
			Node3D projectileParent = new Node3D();
			MultiplayerSpawner projectileSpawner = new MultiplayerSpawner();
			projectileSpawner.Name = "SPAWNER";

			AddChild(projectileParent);
			projectileParent.AddChild(projectileSpawner);
			projectileParent.SetMultiplayerAuthority((int)players[i]);
			projectileParent.Name = "PP" + players[i];

			projectileSpawner.SpawnPath = projectileParent.GetPath();
			playerInfo.characterNode = currentPlayer;
			playerInfo.projectileSpawner = projectileSpawner;
			playerInfo.projectileParent = projectileParent;
			GameManager.Players[players[i]] = playerInfo;

			if(SpawnPoints != null && SpawnPoints.Count > i)
			{
				currentPlayer.GlobalPosition = SpawnPoints[i].GlobalPosition;
			}
		}
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RespawnPlayer(long peerId)
	{
		GameManager.Players[peerId].characterNode.QueueFree();
		GameManager.Players[peerId].characterNode.Name = "QUEUED FOR DESTRUCTION"; // so we dont overlap the id name

		Character currentPlayer = (Character) PlayerScene.Instantiate();
		currentPlayer.Name = peerId.ToString();
		AddChild(currentPlayer);

		var random = new Random();
		if(Multiplayer.GetUniqueId() == peerId)
		{
			currentPlayer.GlobalPosition = SpawnPoints[random.Next(SpawnPoints.Count)].GlobalPosition;
		}

		GameManager.Players[peerId].characterNode = currentPlayer;
	}

    public override void _Process(double delta)
    {
		if(Multiplayer.IsServer())
		{
			foreach(long id in GameManager.Players.Keys)
			{
				if(GameManager.Players[id].characterNode.CurrentHealth == 0)
				{
					Rpc(MethodName.RespawnPlayer, id);
				}
			}
		}
    }


}
