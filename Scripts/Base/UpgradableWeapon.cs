using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public enum FireModes
    {
        // Some weapons with "None" will call "Fire()" in their animantion Ex. Melee weapons
        None,
        Single,
        Automatic,
        Charge
    }
    public partial class UpgradableWeapon : Node3D
    {
        // Define new weapon parameters + projectile as Godot Properties
        [Export]
        public string WeaponName { get; set; } = "Unknown";
        [Export]
        public int MaxAmmo { get; set; } = -1;
        [Export]
        public FireModes FireMode { get; set; } = FireModes.Single;
        [Export]
        public int ProjectilePerShot { get; set; } = 1;
        [Export]
        public Node3D ProjectileOrigin { get; set; }
        [Export, ExportRequired]
        public PackedScene Projectile { get; set; }
        [Export]
        public bool CanFire = true; // Trigger this in the Animation to set the rate of fire
        [Export, ExportRequired, AnimationsRequired(new string[] { "Fire" })] 
        public AnimationPlayer APlayer { get; set; }

        private int CurrentAmmo { get; set; }
        public List<Upgrade> Upgrades { get; set; }

        public override void _Ready()
        {
            CurrentAmmo = MaxAmmo;
            GodotErrorService.ValidateRequiredData(this);
        }

        public void Fire()
        {
            if ((CurrentAmmo > 0 || MaxAmmo < 0) && CanFire) 
            {
                APlayer.Play("Fire");
                CurrentAmmo -= 1;
                for (int x = ProjectilePerShot; x > 0; x--)
                {
                    UpgradeableProjectile projectileInstance = Projectile.Instantiate() as UpgradeableProjectile;
                    projectileInstance.GlobalTransform = ProjectileOrigin.GlobalTransform;
                    GetTree().CurrentScene.AddChild(projectileInstance);
                }
            }
        }

        // Triggered by character
        public void Reload()
        {
            if (APlayer.HasAnimation("Reload"))
            {
                APlayer.Play("Reload");
            }
            else
            {
                GD.PushWarning($"No reload animation for {WeaponName}");
            }
        }

        // Called by Reload Animation or other
        public void RefillAmmo()
        {
            CurrentAmmo = MaxAmmo;
        }

        public string GetCurrentAmmoText()
        {
            return $"{CurrentAmmo}/{MaxAmmo}";
        }
    }
}
