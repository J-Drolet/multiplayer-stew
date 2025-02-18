using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class LevelManager : Node
{   
	[Export]
	public string PlayerStatJson;

	public Dictionary<long, PlayerStat> PlayerStats = new();

	public Dictionary<long, LevelPeerInfo> LevelPeerInfo = new();

	[Export, ExportRequired]
	public PackedScene PlayerScene { get; set; }

	[Export]
	public bool ChangingSpawns { get; set; } = true;

	private bool GameOver = false;

	private Random RNG = new();

	public List<Node3D> SpawnPoints;

	private Node3D PlayerContainer;

	public static LevelManager Instance;

	private double GameTime; // keeps track on the server if the game is over yet
	private double BetweenRoundTime; // server keeps track of time between end game and kick back to lobby

	public override void _Ready() 
	{   
		GodotErrorService.ValidateRequiredData(this);
		Instance = this;

		SpawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoint").Select(s => s as Node3D).ToList();

		foreach(long peerId in GameSessionManager.ConnectedPeers.Keys)
		{   
			SetupPlayerStructure(peerId); // everyone needs to have this structure to sync projectiles

			if(Multiplayer.IsServer()) // only server needs to handle respawns
			{
				// Setup player stats
				PlayerStats.Add(peerId, new());
				PlayerStats[peerId].kills = 0;
				PlayerStats[peerId].deaths = 0;
				PlayerStats[peerId].maxPowerLevel = 0;
				PlayerStats[peerId].aura = 0;
				PlayerStats[peerId].spawnNumber = 0;

				RespawnPlayer(peerId);
			}

		}
		
		if(Multiplayer.IsServer())
		{
			GameTime = 0;
			GameOver = false; // mark the game as not over
		}

		TreeExiting += OnLevelClose;
	}

    private void OnLevelClose()
    {
		// clean up character nodes (they are not a child of the level)
        foreach(LevelPeerInfo peerInfo in LevelPeerInfo.Values)
		{
			peerInfo.characterNode?.QueueFree();
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
		LevelPeerInfo.Add(peerId, new());
		LevelPeerInfo[peerId].projectileSpawner = projectileSpawner;
		LevelPeerInfo[peerId].projectileParent = projectileParent;
	}


	public void RespawnPlayer(long peerId)
	{
		GD.Print("LevelManager.RespawnPlayer - Spawning peer: " + peerId);
		
		Character oldCharacter = LevelPeerInfo[peerId].characterNode;
		if(oldCharacter != null)
		{
			oldCharacter.QueueFree();
		}

		Character newPlayer = (Character) PlayerScene.Instantiate();
		newPlayer.Name = $"{peerId.ToString()}#{++PlayerStats[peerId].spawnNumber}";
		Root.Instance.AddChild(newPlayer, true);

		if(ChangingSpawns || LevelPeerInfo[peerId].spawnPoint == null) // give option to have ever moving spawns or static spawns
		{
			Node3D spawnPoint = SpawnPoints[RNG.Next(SpawnPoints.Count)];
			LevelPeerInfo[peerId].spawnPoint = spawnPoint;
		}
	}

    public override void _Process(double delta)
    {
		if(GameOver) {
			BetweenRoundTime += delta;
			if(BetweenRoundTime >= (double)Config.GetValue("game_constants", "seconds_between_games", true))
			{
				Rpc(MethodName.ReturnToLobby);
				QueueFree(); // delete scene when we return to lobby
			}

			return;
		}

		if(Multiplayer.IsServer())
		{
			PlayerStatJson = JsonSerializer.Serialize(PlayerStats);

			int maxAura = 0;
			foreach(long id in LevelPeerInfo.Keys)
			{
				int playerAura = PlayerStats[id].aura;
				maxAura = playerAura > maxAura ? playerAura : maxAura;

				Character character = LevelPeerInfo[id].characterNode;
				if(character != null) {
					if(character.CurrentHealth == 0)
					{
						PlayerStats[id].deaths++;

						if(PlayerStats.ContainsKey(character.LastDamagedBy))
						{
							PlayerStats[character.LastDamagedBy].kills++;
							PlayerStats[character.LastDamagedBy].aura += LevelPeerInfo[id].characterNode.CalculatePowerLevel();
						}

						RespawnPlayer(id);

						/*
						SendPlayerStats((int)id);
						SendPlayerStats(character.LastDamagedBy);
						*/
					}
				}
			}

			GameTime += delta;
			// End game code
			if(GameTime >= GameSessionManager.GameDurationSeconds || maxAura >= GameSessionManager.MaxAura) 
			{
				GameOver = true;
				Rpc(MethodName.NotifyEndGame);
				BetweenRoundTime = 0;
			}
		}
		else 
		{
			if(PlayerStatJson.Length != 0)
			{
				PlayerStats = JsonSerializer.Deserialize<Dictionary<long, PlayerStat>>(PlayerStatJson);
			}
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RequestSpawnPoint()
	{
		long peerId = Multiplayer.GetRemoteSenderId();
		LevelPeerInfo[peerId].characterNode.RpcId(peerId, Character.MethodName.SetSpawnPoint, LevelPeerInfo[peerId].spawnPoint.GlobalPosition, LevelPeerInfo[peerId].spawnPoint.GlobalRotation);
	}
/*
	public void SendPlayerStats(int peerId)
	{
		Rpc(MethodName.SetPlayerStats, peerId, GameManager.Players[peerId].kills, GameManager.Players[peerId].deaths, GameManager.Players[peerId].maxPowerLevel, GameManager.Players[peerId].aura);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetPlayerStats(int peerId, int kills, int deaths, int maxPowerLevel, int aura)
	{
		if(GameManager.Players.ContainsKey(peerId))
		{
			GameManager.Players[peerId].kills = kills;
			GameManager.Players[peerId].deaths = deaths;
			GameManager.Players[peerId].maxPowerLevel = maxPowerLevel;
			GameManager.Players[peerId].aura = aura;
		}
	}
	*/

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NotifyEndGame()
	{
		Character localCharacter = LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode;
		localCharacter.CanLook = false;
		localCharacter.CanMove = false;

		UI.EndOfGame.Show();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReturnToLobby()
	{
		foreach(LevelPeerInfo peerInfo in LevelPeerInfo.Values)
		{
			if(peerInfo.characterNode != null)
			{
				peerInfo.characterNode.QueueFree();
				peerInfo.characterNode = null;
			}
		}
		UI.EndOfGame.Hide();
		UI.InGameUI.Hide();
		UI.MainMenu.OpenMainMenu();
		UI.Lobby.Show();
	}
}
