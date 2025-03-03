using Godot;

public class ProjectileSpawnInfo {

    public long ProjectileOwner { get; set; }

    public long RandomizerSeed { get; set; }

    public GodotJson.SerializableVector3 SpawnPosition { get; set; }
    public GodotJson.SerializableVector3 SpawnRotation { get; set; }

    public double SpawnTime { get; set; }

    public float AngleOffset { get; set; }
}