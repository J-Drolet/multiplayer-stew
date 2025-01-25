using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Linq;

public partial class SceneManager : Node
{   
    [Export, ExportRequired]
    public PackedScene PlayerScene { get; set; }

    public override void _Ready() 
    {   
        GodotErrorService.ValidateRequiredData(this);

        // spawning characters on spawn points (ran on every peer)
        long[] players = GameManager.Players.Keys.ToArray();
        for(int i = 0; i < players.Length; i++)
        {   
            if(players[i] == 1) {
                continue; // skip the server
            }
            GameManager.PlayerInfo playerInfo = GameManager.Players[players[i]];

            CharacterBody3D currentPlayer = (CharacterBody3D) PlayerScene.Instantiate();
            currentPlayer.Name = playerInfo.id.ToString();
            AddChild(currentPlayer);

            playerInfo.characterNode = currentPlayer;
            GameManager.Players[players[i]] = playerInfo;
            foreach(Node3D spawn in GetTree().GetNodesInGroup("PlayerSpawnPoint"))
            {
                if(spawn.Name == i.ToString())
                {
                    currentPlayer.GlobalPosition = spawn.GlobalPosition;
                }
            }
        }
    }
    
}
