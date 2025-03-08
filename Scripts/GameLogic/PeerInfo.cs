using Godot;
using System;
using System.Collections.Generic;

[Serializable]
public class PeerInfo
{
    public string name;
    public int sequenceNumber; // order of joined players
    public double AveragePing = -1; // in seconds
    public List<double> LastPings = new(); // for handling average pings
}