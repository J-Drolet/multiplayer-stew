using Godot;
using System;


namespace multiplayerstew.Scripts.Base
{
    public abstract class Upgrade
    {

        public abstract void OnUpgradePickup(Character character);

        public abstract void OnFire(UpgradableWeapon weapon);

        public abstract void OnProjectileSpawn(UpgradeableProjectile projectile);

        public abstract void OnProjectileMovement(UpgradeableProjectile projectile);

    }
}