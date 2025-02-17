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

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        UI.Scoreboard = this;
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

        foreach(KeyValuePair<long, GameManager.PlayerInfo> peer in GameManager.Players.OrderBy(x => x.Value.aura).ThenByDescending(x => x.Value.deaths).ThenBy(x => x.Value.name))
        {
            ScoreboardInfo peerInfo = (ScoreboardInfo) PeerPrototype.Duplicate();
            peerInfo.Name = peer.Key.ToString();

            peerInfo.nameLabel.Text = peer.Value.name;
            peerInfo.killsLabel.Text = peer.Value.kills.ToString();
            peerInfo.deathsLabel.Text = peer.Value.deaths.ToString();
            peerInfo.maxPowerLevelLabel.Text = peer.Value.maxPowerLevel.ToString();
            peerInfo.auraLabel.Text = peer.Value.aura.ToString();
            //peerInfo.pingLabel.Text = peer.Value.ping;

            PeerPrototype.AddSibling(peerInfo);
            peerInfo.Show();
        }
    }

}
