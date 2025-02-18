using Godot;
using System;

[Serializable]
public class PeerInfo
{
    public string name;
    public int sequenceNumber; // order of joined players
}


/*public class PlayerInfo {
        public string name;
        public int sequenceNumber; // order of joined players
        public int spawnNumber = 0;
        public Character characterNode;
        public Node3D projectileParent;
        public Node3D spawnPoint;
        public MultiplayerSpawner projectileSpawner;

        // Game stats
        public int kills;
        public int deaths;
        public int maxPowerLevel;
        public int aura;
    }
    */