﻿using Godot;
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

        public float DamageTakenThisFrame { get; set; }
        private Vector3 KnockbackGainedSinceSync { get; set; } 
	    public int LastDamagedBy { get; set; }

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
            if(Multiplayer.IsServer())
            {
                // For batching updates to peers
                if(DamageTakenThisFrame != 0) {
                    RpcId(GetMultiplayerAuthority(), MethodName.ApplyDamage, DamageTakenThisFrame);
                    DamageTakenThisFrame = 0;
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
            }

            if (HealthText != null)
            {
                HealthText.Text = CurrentHealth <= 0.0f ? "Dead" : "Health: " + CurrentHealth.ToString();
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
            bool validDamage = true;

            if(this is Character character)
            {
                if(projectile.BouncesRegistered < 1 && projectile.projectileOwner == character.GetMultiplayerAuthority())
                {
                    validDamage = false;
                }

                if(validDamage)
                {
                    // for knockback
                    if(character.Upgrades.Contains(Upgrade.C_SmallerHitbox) && collisionNormal != Vector3.Zero)
                    {
                        Vector3 knockback = -collisionNormal.Normalized() * damage * (float)Config.GetValue("Upgrade.C_SmallerHitbox", "knockback_strength_per_damage", true);
                        KnockbackGainedSinceSync += knockback;
                    }

                    // for slowdown projectiles
                    if(LevelManager.Instance.LevelPeerInfo[projectile.projectileOwner].characterNode.Upgrades.Contains(Upgrade.W_SlowTargetBullets))
                    {
                        character.RpcId(GetMultiplayerAuthority(), Character.MethodName.SetSlowdown, (float)Config.GetValue("Upgrade.W_SlowTargetBullets", "slowdown_speed_multiplier", true));
                    }
                } 
            }

            if(validDamage)
            {
                DamageTakenThisFrame += damage;
                LastDamagedBy = projectile.projectileOwner;
                                        
                projectile.MaxHits--;
                if(projectile.MaxHits <= 0)
                {
                    projectile.DisableSelf();
                }

                LevelManager.Instance.RpcId(projectile.projectileOwner, LevelManager.MethodName.DisplayHitmarker, hitbox.TriggerVital);
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void ApplyDamage(float damage)
        {
            if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast health values

            CurrentHealth = Math.Clamp(CurrentHealth - damage, 0.0f, MaxHealth);
        }
    }
}
