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

        public override void _EnterTree()
        {
            SetMultiplayerAuthority(Name.ToString().Split('#').First().ToInt());
        }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);

            if (!IsMultiplayerAuthority()) return;

            Hitbox.AreaEntered += OnAreaEntered;

            Vector3 directionVector = -GlobalTransform.Basis.Z;

            Random rand = new Random();
            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));

            Velocity = directionVector * InitialVelocity;
        }

        private void OnAreaEntered(Area3D area)
        {
            if(!IsMultiplayerAuthority()) return;


            if(area is DamageArea) 
            {
                DamageArea damageArea = area as DamageArea;
                damageArea.HitDamageArea(this);
            }
        }


        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            TimeAlive += (float)delta;
            if (TimeAlive > Lifespan)
            {
                QueueFree();
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

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void DeletePeerProjectile()
        {
            QueueFree();
        }

    }
}
