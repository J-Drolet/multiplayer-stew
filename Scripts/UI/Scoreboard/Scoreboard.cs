using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class Scoreboard : Control
{
    [Export, ExportRequired]
    public ScoreboardInfo PeerPrototype;

    private string LastUpdatePlayerStats;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        UI.Scoreboard = this;
        VisibilityChanged += OnVisibilityChanged;
    }

    public override void _Process(double delta)
    {
        if(LevelManager.Instance?.PlayerStatJson != LastUpdatePlayerStats)
        {
            LastUpdatePlayerStats = LevelManager.Instance.PlayerStatJson;
            RefreshScoreboard();
        }
    }

    private void OnVisibilityChanged()
    {
        if(Visible && LevelManager.Instance != null)
        {
            LastUpdatePlayerStats = LevelManager.Instance.PlayerStatJson;
            RefreshScoreboard();
        }
    }

    public void RefreshScoreboard()
    {
        // Clear old nodes
        foreach(Node node in PeerPrototype.GetParent().GetChildren())
        {
            if(node != PeerPrototype)
            {
                node.QueueFree();
            }
        }

        Dictionary<long, PlayerStat> playerStats = LevelManager.Instance.PlayerStats;
        List<long> peers = playerStats.Keys.OrderBy(x => playerStats[x].aura).ThenByDescending(x => playerStats[x].deaths).ThenBy(x => GameSessionManager.ConnectedPeers[x].name).ToList();
        foreach(long peerId in peers)
        {
            ScoreboardInfo peerInfo = (ScoreboardInfo) PeerPrototype.Duplicate();
            peerInfo.Name = peerId.ToString();

            peerInfo.nameLabel.Text = GameSessionManager.ConnectedPeers[peerId].name;
            peerInfo.killsLabel.Text = LevelManager.Instance.PlayerStats[peerId].kills.ToString();
            peerInfo.deathsLabel.Text = LevelManager.Instance.PlayerStats[peerId].deaths.ToString();
            peerInfo.maxPowerLevelLabel.Text = LevelManager.Instance.PlayerStats[peerId].maxPowerLevel.ToString();
            peerInfo.auraLabel.Text = LevelManager.Instance.PlayerStats[peerId].aura.ToString();
            peerInfo.pingLabel.Text = LevelManager.Instance.PlayerStats[peerId].ping.ToString();

            PeerPrototype.AddSibling(peerInfo);
            peerInfo.Show();
        }
    }

}
