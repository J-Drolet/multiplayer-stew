using Godot;
using System;
using System.Collections.Generic;

public static class GameSessionManager
{   
    public static Dictionary<long, PeerInfo> ConnectedPeers = new();

    public static GameInfo GameInfo { get; set; }
    public static long GameHost { get; set; } = -1;

    public static void RemovePlayer(long id)
    {
        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.LevelPeerInfo[id].characterNode.QueueFree();
            LevelManager.Instance.LevelPeerInfo.Remove(id);
            LevelManager.Instance.PlayerStats.Remove(id);
        }

        ConnectedPeers.Remove(id);
    }

    public static void LeaveJoinedGame()
    {
        Client.Instance.Disconnect();
        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.QueueFree();
        }

        UI.MainMenu.Show();
        UI.PauseMenu.Hide();
        UI.InGameUI.Hide();
        UI.EndOfGame.Hide();
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
