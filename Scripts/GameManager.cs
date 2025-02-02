using Godot;
using System;
using System.Collections.Generic;

public static class GameManager
{   
    public class PlayerInfo {
        public string name;
        public int sequenceNumber; // order of joined players
        public int spawnNumber = 0;
        public Character characterNode;
        public Node3D projectileParent;
        public Node3D spawnPoint;
        public MultiplayerSpawner projectileSpawner;
    }

    public static long GameHost { get; set; } = -1;

    public static Dictionary<long, PlayerInfo> Players = new Dictionary<long, PlayerInfo>();

    public static Node CurrentLevel { get; set; }
    public static int GameDurationSeconds {get; set; }
    public static int MaxAura {get; set; }

    public static void RemovePlayer(long id)
    {
        if(Players.ContainsKey(id) && Players[id].characterNode != null)
        {
            Players[id].characterNode.QueueFree();
        }

        Players.Remove(id);
    }

    public static void LeaveJoinedGame()
    {
        Client.Instance.Disconnect();
        if(CurrentLevel != null)
        {
            CurrentLevel.QueueFree();
            CurrentLevel = null;
        }

        UI.MainMenu.OpenMainMenu();
        UI.PauseMenu.Hide();
        UI.InGameUI.Hide();
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
