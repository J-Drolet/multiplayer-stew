using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class Entity : CharacterBody3D
    {
        protected float PeerCurrentHealth { get; set; } // this is the current health used by peers to display things
        [Export, ExportRequired]
        public float MaxHealth { get; set; } = 100.0f;
        [Export, ExportRequired]
        public DamageArea[] Hitboxes { get; set; }
        [Export]
        public Label3D HealthText { get; set; }

        public float ServerCurrentHealth { get; set; } // server uses this to keep track of the real current health
        private Vector3 KnockbackGainedSinceSync { get; set; } 

        public void Init()
        {
            if(Multiplayer.IsServer())
            {
                ServerCurrentHealth = MaxHealth;
                // Only server should listen for hits so signals are only hooked up on server
                foreach (DamageArea hitbox in Hitboxes)
                {
                    hitbox.AreaHit += (UpgradeableProjectile projectile, Vector3 collisionNormal) => HitboxHit(projectile, hitbox, collisionNormal);
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
            // For batching updates to peers
            if(ServerCurrentHealth != PeerCurrentHealth) {
                Rpc(MethodName.SetCurrentHealth, ServerCurrentHealth);
            }

            // batch knockback gained
            if(KnockbackGainedSinceSync != Vector3.Zero)
            {
                if(this is Character character)
                {
                    character.RpcId(GetMultiplayerAuthority(), Character.MethodName.AddKnockback, KnockbackGainedSinceSync);
                }
                KnockbackGainedSinceSync = Vector3.Zero;
            }

            if (HealthText != null)
            {
                HealthText.Text = PeerCurrentHealth <= 0.0f ? "Dead" : "Health: " + PeerCurrentHealth.ToString();
            }
        }

        /// <summary>
        /// Only called on the server to handle when a hitbox is hit
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="hitbox"></param>
        /// <param name="collisionNormal"></param>
        public void HitboxHit(UpgradeableProjectile projectile, DamageArea hitbox, Vector3 collisionNormal)
        {
            float damage = projectile.Damage * hitbox.DamageMultiplier * (hitbox.TriggerVital ?  projectile.VitalMultiplier : 1);
            ServerCurrentHealth = Math.Clamp(ServerCurrentHealth - damage, 0.0f, MaxHealth);

            // for knockback
            if(this is Character character)
            {
                if(character.Upgrades.Contains(Upgrade.C_SmallerHitbox) && collisionNormal != Vector3.Zero)
                {
                    Vector3 knockback = -collisionNormal.Normalized() * damage * (float)Config.GetValue("upgrade_constants", "knockback_strength_per_damage", true);
                    KnockbackGainedSinceSync += knockback;
                }
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void SetCurrentHealth(float health)
        {
            if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast health values

            PeerCurrentHealth = health;
        }
    }
}
