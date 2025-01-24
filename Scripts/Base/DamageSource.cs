using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class DamageSource : Node
    {
        [Export, ExportRequired]
        public float MaxHealth { get; set; } = 100.0f;
        [Export, ExportRequired]
        public Area3D[] Hitboxes { get; set; }
        [Export]
        public Label3D HealthText { get; set; }

        private float CurrentHealth { get; set; }
        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
            
            CurrentHealth = MaxHealth;
            if (HealthText != null)
            {
                HealthText.Text = "Health: " + CurrentHealth.ToString();
            }
            foreach (Area3D hitbox in Hitboxes)
            {
                hitbox.AreaEntered += (Area3D projectileHitbox) => HitboxHit(projectileHitbox, hitbox);
            }   
        }

        private void HitboxHit(Area3D projectileHitbox, Area3D hitbox)
        {
            UpgradeableProjectile projectile = projectileHitbox?.GetOwner() as UpgradeableProjectile;
            if (projectile != null)
            {
                // layer 5 = vital hitbox
                float damage = hitbox.GetCollisionLayerValue(5) ? projectile.Damage * projectile.VitalMultiplier : projectile.Damage;
                CurrentHealth = Math.Clamp(CurrentHealth - damage, 0.0f, float.MaxValue);
                if(HealthText != null)
                {
                    HealthText.Text = CurrentHealth <= 0.0f ? "Dead" : "Health: " + CurrentHealth.ToString();
                }
                
                projectile.QueueFree();
            }
        }
    }
}
