using Godot;
using System;
using System.Collections.Generic;

public static class GameManager
{   
    public class PlayerInfo {
        public string name;
        public int sequenceNumber; // order of joined players
        public Node3D characterNode;
        public Node3D projectileParent;
        public MultiplayerSpawner projectileSpawner;
    }

    public static long GameHost { get; set; } = -1;

    public static Dictionary<long, PlayerInfo> Players = new Dictionary<long, PlayerInfo>();

    public static Node CurrentLevel { get; set; }

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
    }

    public static void UpdateAudioBuses()
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb((float) Config.GetValue("settings", "music_volume")));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb((float) Config.GetValue("settings", "sfx_volume")));
    }

}
