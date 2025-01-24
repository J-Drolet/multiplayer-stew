using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;


namespace multiplayerstew.Scripts.Base
{
    public partial class DamageSource : Node
    {
        [Export, ExportRequired]
        public float MaxHealth { get; set; } = 100.0f;
        [Export, ExportRequired]
        public Area3D Hitbox { get; set; }
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
            Hitbox.AreaEntered += HitboxHit; 
        }

        private void HitboxHit(Area3D projectileHitbox)
        {
            UpgradeableProjectile projectile = projectileHitbox?.GetOwner() as UpgradeableProjectile;
            if (projectile != null)
            {
                CurrentHealth = Math.Clamp(CurrentHealth - projectile.Damage, 0.0f, float.MaxValue);
                if(HealthText != null)
                {
                    HealthText.Text = CurrentHealth <= 0.0f ? "Dead" : "Health: " + CurrentHealth.ToString();
                }
                
                projectile.QueueFree();
            }
        }
    }
}
