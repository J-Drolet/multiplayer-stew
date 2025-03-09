using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace multiplayerstew.Scripts.Base
{ 
    public partial class UpgradeableProjectile: Node3D
    {
        [Export, ExportRequired]
        public MultiplayerSynchronizer multiplayerSynchronizer { get; set; }

        public float InitialVelocity { get; set; } = 20.0f;
        public float Damage { get; set; } = 1.0f;
        public float VitalMultiplier { get; set; } = 2.0f;
        public int MaxHits { get; set; } = 1;
        public float Lifespan { get; set; } = 3.0f;
        public float ShotSpread { get; set; } = 0.0f;

        private Vector3 Velocity = Vector3.Zero;
        private Vector3 ProjectileGravity = Vector3.Down * 5;
        private float TimeAlive;
        private int HitsLeft;
        public int BouncesRegistered; // for bouncing projectile upgrade
        private double CatchUpTime = 0;
        private int AngleOffset;
        public bool IsDummy = false; // For fake visual versions

        public int projectileOwner;
        private Random Rng;
        private RayCast3D WorldHitDetectionRaycast; // for detecting if about to hit a world element
        private RayCast3D HitboxDetectionRaycast; // for detecting if hitting a hitbox
        private HashSet<Upgrade> Upgrades = new(); // projectiles keep track of their own upgrades. Don't want projectile behavior to change mid-flight on upgrade pickup

        public override void _EnterTree()
        {
            ProjectileSpawnInfo spawnInfo = GodotJson.FromJson<ProjectileSpawnInfo>(Name.ToString());

            projectileOwner = (int)spawnInfo.ProjectileOwner;
            Rng = new((int)spawnInfo.RandomizerSeed); // get seed from name
            Upgrades = new HashSet<Upgrade>(LevelManager.Instance.LevelPeerInfo[projectileOwner].characterNode.Upgrades);
            Damage = (float)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "damage", true);
            InitialVelocity = (float)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "bullet_velocity", true);
            VitalMultiplier = (float)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "vital_multiplier", true);
            MaxHits = (int)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "max_hits", true);
            Lifespan = (float)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "lifespan", true);
            ShotSpread = (float)Config.GetValue("Weapon." + spawnInfo.WeaponType.ToString(), "shot_spread", true);


            if(IsMultiplayerAuthority())
            {
                GlobalTransform = spawnInfo.SpawnTransform.ToTransform3D();
                AngleOffset = (int)spawnInfo.AngleOffset;
                if(!IsDummy)
                {
                    CatchUpTime = GameSessionManager.ConnectedPeers[projectileOwner].AveragePing;
                }
            }

            // disable syncing on dummy
            if(IsDummy) {
                multiplayerSynchronizer.Free();
            }
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);

            if(!IsMultiplayerAuthority()) return; // server and local peer continue

            Vector3 directionVector = -GlobalTransform.Basis.Z;
            int yAngleOffset = AngleOffset;
            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread) + Mathf.DegToRad(yAngleOffset));

            Velocity = directionVector * InitialVelocity;

            // dynamically add world collision hitbox
            WorldHitDetectionRaycast = new();
            AddChild(WorldHitDetectionRaycast);

            WorldHitDetectionRaycast.CollideWithAreas = false;
            WorldHitDetectionRaycast.CollideWithBodies = true;
            WorldHitDetectionRaycast.HitFromInside = false;
            WorldHitDetectionRaycast.TargetPosition = Vector3.Zero;

            if(IsDummy) return;

            // dynamically add hitbox raycast
            HitboxDetectionRaycast = new();
            AddChild(HitboxDetectionRaycast);
            HitboxDetectionRaycast.SetCollisionMaskValue(1, false);
            HitboxDetectionRaycast.SetCollisionMaskValue(3, true);
            HitboxDetectionRaycast.TargetPosition = Vector3.Zero;

            HitboxDetectionRaycast.CollideWithAreas = true;
            HitboxDetectionRaycast.CollideWithBodies = false;
            HitboxDetectionRaycast.HitFromInside = true;
        }

        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return; // only the server is concerned with destroying bullets

            TimeAlive += (float)delta;
            if (TimeAlive > Lifespan)
            {
                DisableSelf();
                TimeAlive = -10; // give time for RPC to go through before trying to send it again (stops errors)
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            delta = delta + CatchUpTime; // used to speed up projectile for when it was spawned initially
            CatchUpTime = 0;

            Velocity += ProjectileGravity * (float)delta;
            
            // for homing upgrade
            if(Upgrades.Contains(Upgrade.W_Homing))
            {
                Character nearestEnemy = null;
                float maxHomingDistance = (float)Config.GetValue("Upgrade.W_Homing", "homing_max_distance", true);
                foreach(int peerId in LevelManager.Instance.LevelPeerInfo.Keys)
                {
                    if(peerId != projectileOwner)
                    {   
                        float distance = (GlobalPosition - LevelManager.Instance.LevelPeerInfo[peerId].characterNode.GlobalPosition).Length();
                        if(nearestEnemy == null && distance <= maxHomingDistance)
                        {
                            nearestEnemy = LevelManager.Instance.LevelPeerInfo[peerId].characterNode;
                        }
                        else if(nearestEnemy != null)
                        {
                            float oldDistance = (GlobalPosition - nearestEnemy.GlobalPosition).Length();
                            if(distance < oldDistance)
                            {
                                nearestEnemy = LevelManager.Instance.LevelPeerInfo[peerId].characterNode;
                            }
                        }
                    }
                }

                if(nearestEnemy != null)
                {
                    float speed = Velocity.Length();
                    // Define ideal velocity (direction x speed towards player character from current position)
                    Vector3 idealVelocity = (nearestEnemy.Head.GlobalPosition - GlobalPosition).Normalized() * speed;
                    // speed to steer = direction vector obtained by idealVelocity - current_velocity x force to steer
                    Vector3 steering = (idealVelocity - Velocity).Normalized() * (float)Config.GetValue("Upgrade.W_Homing", "homing_intensity", true);
                    Velocity += steering * (float)delta;
                }
            }

            if (Velocity != Vector3.Zero)
            {
                LookAt(GlobalPosition + Velocity.Normalized(), Vector3.Up);

                if(!IsDummy) // only the non dummy projectiles care about hit collision
                {
                    HitboxDetectionRaycast.TargetPosition = ToLocal(GlobalPosition + Velocity * (float)delta);
                    HitboxDetectionRaycast.ForceRaycastUpdate();
                    if(HitboxDetectionRaycast.IsColliding())
                    {
                        GodotObject collider = HitboxDetectionRaycast.GetCollider();
                        if(MaxHits > 0 && collider is DamageArea damageArea) // we do a max hit check here so server stops hitting even before authority queue frees the projectile
                        {
                            damageArea.HitDamageArea(this, HitboxDetectionRaycast.GetCollisionNormal());
                        }
                    }
                }

                // Collision with world detection
                WorldHitDetectionRaycast.TargetPosition = ToLocal(GlobalPosition + Velocity * (float)delta);
                WorldHitDetectionRaycast.ForceRaycastUpdate();
                if(WorldHitDetectionRaycast.IsColliding())
                {
                    if(Upgrades.Contains(Upgrade.W_BouncyProjectile) && BouncesRegistered < (int) Config.GetValue("Upgrade.W_BouncyProjectile", "bounce_max_bounces", true))
                    {
                        Vector3 collisionPoint = WorldHitDetectionRaycast.GetCollisionPoint();
                        Vector3 collisionNormal = WorldHitDetectionRaycast.GetCollisionNormal();
                        
                        float lengthToCollision = (GlobalPosition - collisionPoint).Length();
                        float percentageDeltaToCollision = lengthToCollision / (Velocity * (float)delta).Length();
                        float remaingDelta = (1 - percentageDeltaToCollision) * (float) delta; // only give the remaining delta for moving
                        
                        GlobalPosition = collisionPoint;
                        Velocity = Velocity - 2 * Velocity.Dot(collisionNormal) * collisionNormal;
                        delta = remaingDelta;
                        BouncesRegistered++;
                    }
                    else // always destroy on hit wall
                    {
                        DisableSelf();
                    }

                }

                GlobalPosition += Velocity * (float)delta;
            }
        }

        /// <summary>
        /// Way for the server to kill a projectile without having authority.
        /// </summary>                        
        public void DisableSelf()
        {
            if(!IsMultiplayerAuthority()) return;

            if(GetMultiplayerAuthority() == projectileOwner)
            {
                QueueFree();
            }
            else
            {
                RpcId(projectileOwner, MethodName.NotifyOfDestruction);
                SetMultiplayerAuthority(projectileOwner); // stops server from syncing position - stops errors from desync between time that projectileOwner queueFree and the server gets the message
            }
            
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void NotifyOfDestruction()
        {
            if(Multiplayer.GetRemoteSenderId() == 1) // only server should send this
            {
                QueueFree();
            }
        }
    }
}
