using Godot;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class UpgradableWeapon: Node3D
    {
        // Define new weapon parameters + projectile as Godot Properties
        [Export]
        public int MaxAmmo;
        [Export]
        public float FireRate;
        [Export]
        public int ProjectilePerShot;
        [Export]
        public Node3D ProjectileOrigin;
        [Export]
        public PackedScene Projectile;

        public int CurrentAmmo;
        public bool CanFire;
        public List<Upgrade> Upgrades;

        public override void _Ready()
        {
            CurrentAmmo = MaxAmmo;
        }

        public void Fire()
        {
            if (CurrentAmmo > 0 || MaxAmmo < 0) 
            {
                for (int x = ProjectilePerShot; x > 0; x--)
                {
                    
                    UpgradeableProjectile projectileInstance = Projectile.Instantiate() as UpgradeableProjectile;
                    projectileInstance.GlobalTransform = ProjectileOrigin.GlobalTransform;
                    GetTree().CurrentScene.AddChild(projectileInstance);
                }
            }
        }
    }
}
