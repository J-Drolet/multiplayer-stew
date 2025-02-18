using System;
using Godot;

[Serializable]
public class LevelPeerInfo
{
    public Character characterNode;
    public Node3D projectileParent;
    public Node3D spawnPoint;
    public MultiplayerSpawner projectileSpawner;
}