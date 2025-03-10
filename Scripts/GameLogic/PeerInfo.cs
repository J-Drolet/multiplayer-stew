using Godot;
using System;
using System.Collections.Generic;

[Serializable]
public class PeerInfo
{
    public string name { get; set; }
    public int sequenceNumber { get; set; } // order of joined players
    public double AveragePing = -1; // in seconds
    public List<double> LastPings = new(); // for handling average pings
    public string FaceCosmetic { get; set; } = "";
    public string HeadCosmetic { get; set; } = "";
}