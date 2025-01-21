using Godot;
using System;
using System.Collections.Generic;

public static class GameManager
{   
    public struct PlayerInfo {
        public string name;
        public long id;
        public Node3D characterNode;
    }

    public static Dictionary<long, PlayerInfo> Players = new Dictionary<long, PlayerInfo>();

    public static void RemovePlayer(long id)
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
