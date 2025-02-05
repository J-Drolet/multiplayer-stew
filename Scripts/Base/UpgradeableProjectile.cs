using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
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

        private Vector3 Velocity = Vector3.Zero;
        private Vector3 ProjectileGravity = Vector3.Down * 5;
        private float TimeAlive;
        private int HitsLeft;

        private int projectileOwner;
        private Random Rng;
        private RayCast3D HitDetectionRaycast;

        public override void _EnterTree()
        {
            projectileOwner = Name.ToString().Split('#').First().ToInt();
            Rng = new(Name.ToString().Split('#').Last().ToInt()); // get seed from name
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);

            if(projectileOwner == Multiplayer.GetUniqueId()) {
                SetMultiplayerAuthority(projectileOwner);
                multiplayerSynchronizer.Free();
            }

            if(!IsMultiplayerAuthority()) return;

            GlobalTransform = GameManager.Players[projectileOwner].characterNode.ProjectileOrigin.GlobalTransform;

            Vector3 directionVector = -GlobalTransform.Basis.Z;

            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)Rng.NextDouble() * ShotSpread*2) + (-ShotSpread));

            Velocity = directionVector * InitialVelocity;

            if (!Multiplayer.IsServer()) return;
            Func<int, bool> syncFilter = (int peerId) => { return peerId != projectileOwner; };
            multiplayerSynchronizer.AddVisibilityFilter(Callable.From(syncFilter));

            // dynamically add raycast
            HitDetectionRaycast = new();
            AddChild(HitDetectionRaycast);
            HitDetectionRaycast.SetCollisionMaskValue(1, false);
            HitDetectionRaycast.SetCollisionMaskValue(3, true);
            HitDetectionRaycast.SetCollisionMaskValue(5, true);

            HitDetectionRaycast.CollideWithAreas = true;
            HitDetectionRaycast.CollideWithBodies = false;
            HitDetectionRaycast.HitFromInside = true;
        }

        public override void _Process(double delta)
        {
            if (!Multiplayer.IsServer()) return;

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

            Velocity += ProjectileGravity * (float)delta;

            if (Velocity != Vector3.Zero)
            {
                LookAt(GlobalPosition + Velocity.Normalized(), Vector3.Up);

                if(Multiplayer.IsServer())
                {
                    HitDetectionRaycast.TargetPosition = ToLocal(GlobalPosition + Velocity * (float)delta);
                    HitDetectionRaycast.ForceRaycastUpdate();
                    if(HitDetectionRaycast.IsColliding())
                    {
                        GodotObject collider = HitDetectionRaycast.GetCollider();
                        if(MaxHits > 0 && collider is DamageArea) // we do a max hit check here so server stops hitting even before authority queue frees the projectile
                        {
                            DamageArea damageArea = collider as DamageArea;
                            damageArea.HitDamageArea(this);
                        }
                        
                        MaxHits--;
                        
                        if(MaxHits <= 0)
                        {
                            DisableSelf();
                        }
                    }
                }

                GlobalPosition += Velocity * (float)delta;
            }
        }

        /// <summary>
        /// Way for the server to kill a projectile without having authority.
        /// </summary>                        
        private void DisableSelf()
        {
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
    }
}
