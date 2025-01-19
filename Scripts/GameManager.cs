using Godot;
using System;
using System.Collections.Generic;

public static class GameManager
{   
    public struct PlayerInfo {
        public string name;
        public int id;
        public Node3D characterNode;
    }

    public static Dictionary<int, PlayerInfo> Players = new Dictionary<int, PlayerInfo>();

    public static void RemovePlayer(int id)
    {
        if(Players.ContainsKey(id) && Players[id].characterNode != null)
        {
            Players[id].characterNode.QueueFree();
        }

        Players.Remove(id);
    }

    public static void UpdateAudioBuses()
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(Settings.GetValue("music_volume").ToFloat()));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb(Settings.GetValue("sfx_volume").ToFloat()));
    }

}
