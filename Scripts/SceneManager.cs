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

	[Export]
	public bool ChangingSpawns { get; set; } = true;

	private Random RNG = new();

	public List<Node3D> SpawnPoints;

	private Node3D PlayerContainer;

	public static SceneManager Instance;

	public override void _Ready() 
	{   
		GodotErrorService.ValidateRequiredData(this);
		Instance = this;

		SpawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoint").Select(s => s as Node3D).ToList();

		foreach(long key in GameManager.Players.Keys)
		{   
			if(Multiplayer.IsServer()) // only server needs to handle respawns
			{
				RespawnPlayer(key);
			}

			SetupPlayerStructure(key); // everyone needs to have this structure to sync projectiles
		}

		
		if(!Multiplayer.IsServer()) 
		{
			UI.InGameUI.Show();
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	public void SetupPlayerStructure(long peerId)
	{
		Node3D projectileParent = new();
		MultiplayerSpawner projectileSpawner = new();
		projectileSpawner.Name = "SPAWNER";

		AddChild(projectileParent);
		projectileParent.AddChild(projectileSpawner);
		projectileParent.SetMultiplayerAuthority((int)peerId);
		projectileParent.Name = "PP" + peerId;

		projectileSpawner.SpawnPath = projectileParent.GetPath();
		GameManager.Players[peerId].projectileSpawner = projectileSpawner;
		GameManager.Players[peerId].projectileParent = projectileParent;
	}



	public void RespawnPlayer(long peerId)
	{
		Character oldCharacter = GameManager.Players[peerId].characterNode;
		if(oldCharacter != null)
		{
			oldCharacter.QueueFree();
		}

		Character newPlayer = (Character) PlayerScene.Instantiate();
		newPlayer.Name = $"{peerId.ToString()}#{++GameManager.Players[peerId].spawnNumber}";
		Root.Instance.AddChild(newPlayer, true);

		if(ChangingSpawns || GameManager.Players[peerId].spawnPoint == null) // give option to have ever moving spawns or static spawns
		{
			Node3D spawnPoint = SpawnPoints[RNG.Next(SpawnPoints.Count)];
			GameManager.Players[peerId].spawnPoint = spawnPoint;
		}
	}

    public override void _Process(double delta)
    {
		if(Multiplayer.IsServer())
		{
			foreach(long id in GameManager.Players.Keys)
			{
				//GD.Print("Examining player " + id);
				if(GameManager.Players[id].characterNode != null) {
					if(GameManager.Players[id].characterNode.CurrentHealth == 0)
					{
						GD.Print("Respawning " + id);
						RespawnPlayer(id);
					}
				}
			}
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RequestSpawnPoint()
	{
		long peerId = Multiplayer.GetRemoteSenderId();
		GameManager.Players[peerId].characterNode.RpcId(peerId, Character.MethodName.SetSpawnPoint, GameManager.Players[peerId].spawnPoint.GlobalPosition, GameManager.Players[peerId].spawnPoint.GlobalRotation);
	}
}
