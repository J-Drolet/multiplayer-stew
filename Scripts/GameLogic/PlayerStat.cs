
using System;

[Serializable]
public class PlayerStat
{
    public int spawnNumber { get; set; }= 0; // order in which a player spawned in

    public int kills { get; set; }
    public int deaths { get; set; }
    public int maxPowerLevel { get; set; }
    public int aura { get; set; }
    public int ping { get; set; }
    public ulong lastPinged;
}