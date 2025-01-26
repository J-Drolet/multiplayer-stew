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

    public override void _Ready() 
    {   
        GodotErrorService.ValidateRequiredData(this);

        UI.InGameUI.Show();

        // spawning characters on spawn points (ran on every peer)
        List<long> players = GameManager.Players.Keys.OrderBy(p => p).ToList();
        List<Node3D> spawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoint").Select(s => s as Node3D).ToList();

        for (int i = 0; i < players.Count; i++)
        {   
            if(players[i] == 1) {
                continue; // skip the server
            }
            GameManager.PlayerInfo playerInfo = GameManager.Players[players[i]];

            CharacterBody3D currentPlayer = (CharacterBody3D) PlayerScene.Instantiate();
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

            if(spawnPoints != null && spawnPoints.Count > i)
            {
                currentPlayer.GlobalPosition = spawnPoints[i].GlobalPosition;
            }
        }
    }
    
}
