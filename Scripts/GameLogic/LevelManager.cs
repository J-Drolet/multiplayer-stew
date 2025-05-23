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

		// initialize WorldEnvironment based on settings
		Config.Instance.EmitSignal("ConfigChanged", "settings", "ambient_occlusion", Config.GetValue("settings", "ambient_occlusion"));

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
		else
		{
			UI.LoadingScreen.Hide();
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

			int maxAura = 0;
			foreach(long id in LevelPeerInfo.Keys)
			{
                if (!PlayerStats.ContainsKey(id))
                {
                    GD.PrintErr($"PlayerStats does not contain key: {id}");
                    continue; // Skip processing for this ID
                }

                int playerAura = PlayerStats[id].aura;
				maxAura = playerAura > maxAura ? playerAura : maxAura;

				Character character = LevelPeerInfo[id].characterNode;
				if(character != null) {
					if(character.CurrentHealth == 0)
					{
						PlayerStats[id].deaths++;

						if(character.GetMultiplayerAuthority() != character.LastDamagedBy && PlayerStats.ContainsKey(character.LastDamagedBy))
						{
							int auraGained = LevelPeerInfo[id].characterNode.CalculatePowerLevel();
							PlayerStats[character.LastDamagedBy].kills++;
							PlayerStats[character.LastDamagedBy].aura += auraGained;

							RpcId(character.LastDamagedBy, MethodName.NotifyOfKill, id, auraGained);

							string lastEliminatedName = character.LastDamagedBy == 1 ? "Server" : GameSessionManager.ConnectedPeers[character.LastDamagedBy].name;
							GD.Print("LevelManager._Process - " + GameSessionManager.ConnectedPeers[id].name + " was eliminated by " + lastEliminatedName);
						}
						RespawnPlayer(id);
					}
				}
			}

			GameTime += delta;
			// End game code
			if(GameTime >= GameSessionManager.GameInfo.DurationInSeconds || maxAura >= GameSessionManager.GameInfo.MaxAura) 
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

	public void ToggleXrayEffect(bool enabled = true)
	{
		int localPeer = Multiplayer.GetUniqueId();
		foreach(long peerId in LevelPeerInfo.Keys)
		{
			Character peerCharacter = LevelPeerInfo[peerId].characterNode;
			if(peerId == localPeer || peerCharacter == null) continue;

			XrayMaterialService.ToggleXray(peerCharacter.GetVisualMeshes(), enabled);
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
		localCharacter.CanLook.AddLock("EndGame");
		localCharacter.CanMove.AddLock("EndGame");

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
		UI.MainMenu.Show();
		UI.Lobby.Show();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void DisplayHitmarker(bool isVital)
	{
		UI.Hitmarker.DisplayHitmarker(isVital);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NotifyOfKill(long peerKilledId, int auraGained)
	{
		UI.KillPopups.DisplayKill(GameSessionManager.ConnectedPeers[peerKilledId].name, auraGained);
	}
}
