using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class SceneManager : Node
{   
	[Export, ExportRequired]
	public PackedScene PlayerScene { get; set; }

	[Export]
	public bool ChangingSpawns { get; set; } = true;

	private bool GameOver = false;

	private Random RNG = new();

	public List<Node3D> SpawnPoints;

	private Node3D PlayerContainer;

	public static SceneManager Instance;

	private double GameTime; // keeps track on the server if the game is over yet
	private double BetweenRoundTime; // server keeps track of time between end game and kick back to lobby

	public override void _Ready() 
	{   
		GodotErrorService.ValidateRequiredData(this);
		Instance = this;

		if(GameManager.CurrentLevel == null)
		{
			GameManager.CurrentLevel = this;
		}

		SpawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoint").Select(s => s as Node3D).ToList();

		foreach(long key in GameManager.Players.Keys)
		{   
			if(Multiplayer.IsServer()) // only server needs to handle respawns
			{
				RespawnPlayer(key);
			}

			SetupPlayerStructure(key); // everyone needs to have this structure to sync projectiles
			GameManager.Players[key].kills = 0;
			GameManager.Players[key].deaths = 0;
			GameManager.Players[key].maxPowerLevel = 0;
			GameManager.Players[key].aura = 0;
		}
		GameTime = 0;

		
		if(!Multiplayer.IsServer()) 
		{
			UI.InGameUI.Show();
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else 
		{
			GameOver = false; // mark the game as not over
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
		GD.Print("SceneManager.RespawnPlayer - Spawning peer: " + peerId);
		
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
			int maxAura = 0;
			foreach(long id in GameManager.Players.Keys)
			{
				maxAura += GameManager.Players[id].aura;
				Character character = GameManager.Players[id].characterNode;
				if(character != null) {
					if(character.CurrentHealth == 0)
					{
						GameManager.Players[id].deaths += 1;

						if(GameManager.Players.ContainsKey(character.LastDamagedBy))
						{
							GameManager.Players[character.LastDamagedBy].kills += 1;
							GameManager.Players[character.LastDamagedBy].aura += GameManager.Players[id].characterNode.CalculatePowerLevel();
						}

						RespawnPlayer(id);

						SendPlayerStats((int)id);
						SendPlayerStats(character.LastDamagedBy);
					}
				}
			}

			GameTime += delta;
			// End game code
			if(GameTime >= GameManager.GameDurationSeconds || maxAura >= GameManager.MaxAura) 
			{
				GameOver = true;
				Rpc(MethodName.NotifyEndGame);
				BetweenRoundTime = 0;
			}
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RequestSpawnPoint()
	{
		long peerId = Multiplayer.GetRemoteSenderId();
		GameManager.Players[peerId].characterNode.RpcId(peerId, Character.MethodName.SetSpawnPoint, GameManager.Players[peerId].spawnPoint.GlobalPosition, GameManager.Players[peerId].spawnPoint.GlobalRotation);
	}

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

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void NotifyEndGame()
	{
		Character localCharacter = GameManager.Players[Multiplayer.GetUniqueId()].characterNode;
		localCharacter.CanLook = false;
		localCharacter.CanMove = false;

		UI.EndOfGame.Show();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReturnToLobby()
	{
		foreach(GameManager.PlayerInfo playerInfo in GameManager.Players.Values)
		{
			if(playerInfo.characterNode != null)
			{
				playerInfo.characterNode.QueueFree();
				playerInfo.characterNode = null;
			}
		}
		UI.EndOfGame.Hide();
		UI.InGameUI.Hide();
		UI.MainMenu.OpenMainMenu();
		UI.Lobby.Show();
	}
}
