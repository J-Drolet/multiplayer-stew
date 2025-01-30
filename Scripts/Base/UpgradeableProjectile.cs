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
        public Area3D Hitbox { get; set; }

        private Vector3 Velocity = Vector3.Zero;
        private Vector3 ProjectileGravity = Vector3.Down * 5;
        private float TimeAlive;
        private int HitsLeft;

        private int projectileOwner;

        public override void _EnterTree()
        {
            projectileOwner = Name.ToString().Split('#').First().ToInt();
            Visible = false;
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
            if (!IsMultiplayerAuthority()) return;

            Hitbox.AreaEntered += OnAreaEntered;

            GlobalTransform = GameManager.Players[projectileOwner].characterNode.ProjectileOrigin.GlobalTransform;
            Visible = true; // make bullet invisible until it is moved to the right spot

            Vector3 directionVector = -GlobalTransform.Basis.Z;

            Random rand = new();
            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));

            Velocity = directionVector * InitialVelocity;
        }

        /// <summary>
        /// When Projectile hits something - This is only run on the server (Server handles hit detection)
        /// </summary>
        /// <param name="area"></param>
        private void OnAreaEntered(Area3D area)
        {
            if(MaxHits > 0 && area is DamageArea) // we do a max hit check here so server stops hitting even before authority queue frees the projectile
            {
                DamageArea damageArea = area as DamageArea;
                damageArea.HitDamageArea(this);
            }
            
            MaxHits--;
            
            if(MaxHits <= 0)
            {
                DisableSelf();
            }
        }

        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            TimeAlive += (float)delta;
            if (TimeAlive > Lifespan)
            {
                DisableSelf();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            Velocity += ProjectileGravity * (float)delta;

            if (Velocity != Vector3.Zero)
            {
                LookAt(Transform.Origin + Velocity.Normalized(), Vector3.Up);
                Transform = Transform with { Origin = Transform.Origin + Velocity * (float)delta };

            }
        }

        /// <summary>
        /// Way for the server to kill a projectile without having authority.
        /// </summary>                        
        private void DisableSelf()
        {
            RpcId(projectileOwner, MethodName.NotifyOfDestruction);
            Hitbox.SetDeferred("monitoring", false); // makes projectile no longer cause damage
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
