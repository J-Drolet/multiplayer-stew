using Godot;

public class ProjectileSpawnInfo {

    public long ProjectileOwner { get; set; }

    public long RandomizerSeed { get; set; }

    public GodotJson.SerializableTransform3D SpawnTransform { get; set; }

    public double SpawnTime { get; set; }

    public float AngleOffset { get; set; }
    public Weapon WeaponType { get; set; }
}