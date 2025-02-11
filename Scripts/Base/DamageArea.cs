using Godot;
using multiplayerstew.Scripts.Base;
using System;

public partial class DamageArea : Area3D
{

    [Signal]
    public delegate void AreaHitEventHandler(UpgradeableProjectile projectile, Vector3 collisionNormal);

    [Export]
    public float DamageMultiplier = 1.0f;
    [Export]
    public bool TriggerVital = false;

    public void HitDamageArea(UpgradeableProjectile projectile, Vector3 collisionNormal) {
        EmitSignal("AreaHit", projectile, collisionNormal);
    }

}
