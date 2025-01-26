using Godot;
using multiplayerstew.Scripts.Base;
using System;

public partial class DamageArea : Area3D
{

    [Signal]
    public delegate void AreaHitEventHandler(UpgradeableProjectile projectile);

    public void HitDamageArea(UpgradeableProjectile projectile) {
        EmitSignal("AreaHit", projectile);
    }

}
