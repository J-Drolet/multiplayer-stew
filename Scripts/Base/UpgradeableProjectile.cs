using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;


namespace multiplayerstew.Scripts.Base
{ 
    public partial class UpgradeableProjectile: Node3D
    {
        
        [Export]
        public float InitialVelocity { get; set; } = 20.0f;
        [Export]
        public float Damage { get; set; } = 1.0f;
        [Export]
        public float VitalMultiplier { get; set; } = 2.0f;
        [Export]
        public int MaxHits { get; set; } = 1;
        [Export]
        public float Lifespan { get; set; } = 3.0f;
        [Export]
        public float ShotSpread { get; set; } = 0.0f;
        [Export, ExportRequired]
        public MultiplayerSynchronizer multiplayerSynchronizer { get; set; }
        [Export]
        private Vector3 TrueGlobalPosition = Vector3.Zero;

        private Vector3 Velocity = Vector3.Zero;
        private Vector3 ProjectileGravity = Vector3.Down * 5;
        private float TimeAlive;
        private int HitsLeft;
        private int BouncesRegistered; // for bouncing projectile upgrade

        public int projectileOwner;
        private Random Rng;
        private RayCast3D WorldHitDetectionRaycast; // for detecting if about to hit a world element
        private RayCast3D HitboxDetectionRaycast; // for detecting if hitting a hitbox
        private HashSet<Upgrade> Upgrades = new(); // projectiles keep track of their own upgrades. Don't want projectile behavior to change mid-flight on upgrade pickup

        public override void _EnterTree()
        {
            projectileOwner = Name.ToString().Split('#').First().ToInt();
            Rng = new(Name.ToString().Split('#')[1].ToInt()); // get seed from name
            Upgrades = new HashSet<Upgrade>(GameManager.Players[projectileOwner].characterNode.Upgrades);
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);

            // disable syncing on projectileOwner
            if(Multiplayer.GetUniqueId() == projectileOwner) {
                multiplayerSynchronizer.SetVisibilityFor(1, false);
                multiplayerSynchronizer.QueueFree();
            }
            //if(Multiplayer.GetUniqueId() == 1) {
            //    multiplayerSynchronizer.SetVisibilityFor(projectileOwner, false);
            //}

            if(!HasRightToMoveProjectile()) return; // server and local peer continue

            GlobalTransform = GameManager.Players[projectileOwner].characterNode.ProjectileOrigin.GlobalTransform;

            Vector3 directionVector = -GlobalTransform.Basis.Z;
            int yAngleOffset = Name.ToString().Split('#')[2].ToInt();
            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread) + Mathf.DegToRad(yAngleOffset));

            Velocity = directionVector * InitialVelocity;

            // dynamically add world collision hitbox
            WorldHitDetectionRaycast = new();
            AddChild(WorldHitDetectionRaycast);

            WorldHitDetectionRaycast.CollideWithAreas = false;
            WorldHitDetectionRaycast.CollideWithBodies = true;
            WorldHitDetectionRaycast.HitFromInside = false;

            if (!Multiplayer.IsServer()) return;

            // dynamically add hitbox raycast
            HitboxDetectionRaycast = new();
            AddChild(HitboxDetectionRaycast);
            HitboxDetectionRaycast.SetCollisionMaskValue(1, false);
            HitboxDetectionRaycast.SetCollisionMaskValue(3, true);

            HitboxDetectionRaycast.CollideWithAreas = true;
            HitboxDetectionRaycast.CollideWithBodies = false;
            HitboxDetectionRaycast.HitFromInside = true;
        }

        public override void _Process(double delta)
        {
            if (!Multiplayer.IsServer()) return; // only the server is concerned with destroying bullets

            TimeAlive += (float)delta;
            if (TimeAlive > Lifespan)
            {
                DisableSelf();
                TimeAlive = -10; // give time for RPC to go through before trying to send it again (stops errors)
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!HasRightToMoveProjectile()) return;

            if(!Multiplayer.IsServer() && TrueGlobalPosition != Vector3.Zero) // interpolation logic
            {
                float interpolationStrength = 1.5f * InitialVelocity; // interpolation strength is a function of the bullet speed
                Vector3 positionDifference = TrueGlobalPosition - GlobalPosition;
                Vector3 movementVector = positionDifference.Normalized() * (float)(interpolationStrength * delta);
                if(positionDifference.LengthSquared() <= movementVector.LengthSquared()) {
                    movementVector = positionDifference;
                }

                GlobalPosition += movementVector;

                /*
                // Calculate the difference between the local and true global positions
                Vector3 positionDifference = TrueGlobalPosition - GlobalPosition;
                Vector3 forward = -GlobalTransform.Basis.Z.Normalized(); // Bullet's forward direction
                Vector3 forwardDisplacement = positionDifference.Project(forward);
                Vector3 diffAfterForward = positionDifference - forwardDisplacement;

                // Interpolate the X and Y components only
                GlobalPosition += diffAfterForward.Normalized() * (float)(delta * 0.000000001);//(float)(delta * (float)Config.GetValue("upgrade_constants", "multiplayer_sync_interpolation_smoothing", true));
                GlobalPosition += forwardDisplacement;
                */
 
                return; // if we are interpolating then we return here
            }

            Velocity += ProjectileGravity * (float)delta;
            
            // for homing upgrade
            if(Upgrades.Contains(Upgrade.W_Homing))
            {
                Character nearestEnemy = null;
                float maxHomingDistance = (float)Config.GetValue("upgrade_constants", "homing_max_distance", true);
                foreach(int peerId in GameManager.Players.Keys)
                {
                    if(peerId != projectileOwner)
                    {   
                        float distance = (GlobalPosition - GameManager.Players[peerId].characterNode.GlobalPosition).Length();
                        if(nearestEnemy == null && distance <= maxHomingDistance)
                        {
                            nearestEnemy = GameManager.Players[peerId].characterNode;
                        }
                        else if(nearestEnemy != null)
                        {
                            float oldDistance = (GlobalPosition - nearestEnemy.GlobalPosition).Length();
                            if(distance < oldDistance)
                            {
                                nearestEnemy = GameManager.Players[peerId].characterNode;
                            }
                        }
                    }
                }

                if(nearestEnemy != null)
                {
                    float speed = Velocity.Length();
                    // Define ideal velocity (direction x speed towards player character from current position)
                    Vector3 idealVelocity = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized() * speed;
                    // speed to steer = direction vector obtained by idealVelocity - current_velocity x force to steer
                    Vector3 steering = (idealVelocity - Velocity).Normalized() * (float)Config.GetValue("upgrade_constants", "homing_intensity", true);
                    Velocity += steering * (float)delta;
                }
            }

            /*
            if(!Multiplayer.IsServer() && TrueGlobalPosition != Vector3.Zero) // add homing towards real position
            {
                float speed = Velocity.Length();
                
                float distanceToTarget = (TrueGlobalPosition - GlobalPosition).Length();
                Vector3 directionToTarget = (TrueGlobalPosition - GlobalPosition).Normalized();
                // Define ideal velocity (direction x speed towards player character from current position)
                Vector3 idealVelocity = directionToTarget * speed;
                
                // Ensure that the ideal velocity doesn't make us overshoot the target
                if (distanceToTarget < speed * delta) {
                    idealVelocity = directionToTarget * (float)(distanceToTarget / delta); // Adjust to stop right at the target
                }

                // speed to steer = direction vector obtained by idealVelocity - current_velocity x force to steer
                Vector3 steering = (idealVelocity - Velocity).Normalized() * (float)Config.GetValue("upgrade_constants", "ping_correction_homing_intensity", true);
                Velocity += steering * (float)delta;
            }
            */
                

            if (Velocity != Vector3.Zero)
            {
                LookAt(GlobalPosition + Velocity.Normalized(), Vector3.Up);

                if(Multiplayer.IsServer()) // only the server cares about hit collision
                {
                    HitboxDetectionRaycast.TargetPosition = ToLocal(GlobalPosition + Velocity * (float)delta);
                    HitboxDetectionRaycast.ForceRaycastUpdate();
                    if(HitboxDetectionRaycast.IsColliding())
                    {
                        GodotObject collider = HitboxDetectionRaycast.GetCollider();
                        if(MaxHits > 0 && collider is DamageArea) // we do a max hit check here so server stops hitting even before authority queue frees the projectile
                        {
                            DamageArea damageArea = collider as DamageArea;
                            damageArea.HitDamageArea(this, HitboxDetectionRaycast.GetCollisionNormal());
                        }
                        
                        MaxHits--;
                        
                        if(MaxHits <= 0)
                        {
                            DisableSelf();
                        }
                    }
                }

                // Collision with world detection
                WorldHitDetectionRaycast.TargetPosition = ToLocal(GlobalPosition + Velocity * (float)delta);
                WorldHitDetectionRaycast.ForceRaycastUpdate();
                if(WorldHitDetectionRaycast.IsColliding())
                {
                    if(Upgrades.Contains(Upgrade.W_BouncyProjectile) && BouncesRegistered < (int) Config.GetValue("upgrade_constants", "bounce_max_bounces", true))
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
                if(Multiplayer.IsServer()) {
                    TrueGlobalPosition = GlobalPosition;
                }
            }
        }

        /// <summary>
        /// Way for the server to kill a projectile without having authority.
        /// </summary>                        
        private void DisableSelf()
        {
            if(!Multiplayer.IsServer()) return;

            RpcId(projectileOwner, MethodName.NotifyOfDestruction);
            SetMultiplayerAuthority(projectileOwner); // stops server from syncing position - stops errors from desync between time that projectileOwner queueFree and the server gets the message
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void NotifyOfDestruction()
        {
            if(Multiplayer.GetRemoteSenderId() == 1) // only server should send this
            {
                QueueFree();
            }
        }

        private bool HasRightToMoveProjectile()
        {
            return IsMultiplayerAuthority() || Multiplayer.GetUniqueId() == projectileOwner;
        }
    }
}
