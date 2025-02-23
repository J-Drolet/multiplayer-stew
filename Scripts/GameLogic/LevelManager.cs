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
	[Export]
	private double GameTime; // keeps track on the server if the game is over yet
	[Export]
	private double BetweenRoundTime; // server keeps track of time between end game and kick back to lobby
	private bool GameOver = false;
	private double TimeSinceLastPing;

	public Dictionary<long, PlayerStat> PlayerStats = new();

	public Dictionary<long, LevelPeerInfo> LevelPeerInfo = new();

	[Export, ExportRequired]
	public PackedScene PlayerScene { get; set; }

	[Export]
	public bool ChangingSpawns { get; set; } = true;

	private Random RNG = new(); // easier to keep here for randomizing spawns

	public List<Node3D> SpawnPoints;

	public static LevelManager Instance;

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
		if(Multiplayer.IsServer())
		{
			PlayerStatJson = JsonSerializer.Serialize(PlayerStats);
			if(GameOver) {
				BetweenRoundTime += delta;
				if(BetweenRoundTime >= (double)Config.GetValue("game_constants", "seconds_between_games", true))
				{
					Rpc(MethodName.ReturnToLobby);
					QueueFree(); // delete scene when we return to lobby
				}

				return;
			}

			bool shouldPingThisFrame = false;
			TimeSinceLastPing += delta;
			if(TimeSinceLastPing * 1000 >= (double)Config.GetValue("game_constants", "server_ping_interval_ms", true))
			{
				TimeSinceLastPing = 0;
				shouldPingThisFrame = true;
			}

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
					}
				}

				if(shouldPingThisFrame && PlayerStats[id].lastPinged == 0) { // don't ping again if still awaiting an answer back
					PlayerStats[id].lastPinged = Time.GetTicksMsec();
					RpcId(id, MethodName.Ping);
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
		else // Only local peer code
		{
			if (!String.IsNullOrEmpty(PlayerStatJson) &&  PlayerStatJson.Length != 0)
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Ping()
	{
		int senderId = Multiplayer.GetRemoteSenderId();
		if(senderId == 1) 
		{
			RpcId(1, MethodName.Ping);
		}
		else
		{
			PlayerStats[senderId].ping = (int)(Time.GetTicksMsec() - PlayerStats[senderId].lastPinged);
			PlayerStats[senderId].lastPinged = 0; // placeholder saying we got the ping
		}
	}
}
