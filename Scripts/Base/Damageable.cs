using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class Damageable : Node
    {
        [Export, ExportRequired]
        public float MaxHealth { get; set; } = 100.0f;
        [Export, ExportRequired]
        public DamageArea[] Hitboxes { get; set; }
        [Export]
        public Label3D HealthText { get; set; }

        [Export]
        public float CurrentHealth { get; set; }
        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
            if (!Multiplayer.IsServer())
                return;

            CurrentHealth = MaxHealth;
            foreach (DamageArea hitbox in Hitboxes)
            {
                hitbox.AreaHit += (UpgradeableProjectile projectile) => HitboxHit(projectile, hitbox);
            }   
        }

        public override void _Process(double delta)
        {
            if (HealthText != null)
            {
                HealthText.Text = CurrentHealth <= 0.0f ? "Dead" : "Health: " + CurrentHealth.ToString();
            }
        }

        private void HitboxHit(UpgradeableProjectile projectile, DamageArea hitbox)
        {
            // layer 5 = vital hitbox
            float damage = hitbox.GetCollisionLayerValue(5) ? projectile.Damage * projectile.VitalMultiplier : projectile.Damage;
            CurrentHealth = Math.Clamp(CurrentHealth - damage, 0.0f, MaxHealth);
            projectile.MaxHits--;

            // Disable projectile from hitting other hitboxes before getting deleted by peer owner
            if(projectile.MaxHits <= 0)
            {
                projectile.SetDeferred("monitorable", false);
                projectile.RpcId(projectile.GetMultiplayerAuthority(), UpgradeableProjectile.MethodName.DeletePeerProjectile);
            }
        }
    }
}
