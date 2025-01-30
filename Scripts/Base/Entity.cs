using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class Entity : CharacterBody3D
    {
        [Export]
        public float CurrentHealth { get; set; }
        [Export, ExportRequired]
        public float MaxHealth { get; set; } = 100.0f;
        [Export, ExportRequired]
        public DamageArea[] Hitboxes { get; set; }
        [Export]
        public Label3D HealthText { get; set; }

        public void Init()
        {

            if(IsMultiplayerAuthority())
            {
                CurrentHealth = MaxHealth;
            }

            if(Multiplayer.IsServer())
            {
                // Only server should listen for hits so signals are only hooked up on server
                foreach (DamageArea hitbox in Hitboxes)
                {
                    hitbox.AreaHit += (UpgradeableProjectile projectile) => HitboxHit(projectile, hitbox);
                }
            }
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
            Init();
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
            RpcId(GetMultiplayerAuthority(), MethodName.SetCurrentHealth, Math.Clamp(CurrentHealth - damage, 0.0f, MaxHealth));
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void SetCurrentHealth(float health)
        {
            if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast health values

            CurrentHealth = health;
        }
    }
}
