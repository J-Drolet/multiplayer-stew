using Godot;
using System;


namespace multiplayerstew.Scripts.Base
{ 
    public partial class UpgradeableProjectile: Node3D
    {
        
        [Export]
        public float InitialVelocity;
        [Export]
        public float Damage;
        [Export]
        public float VitalMultiplier;
        [Export]
        public int MaxHits = 1;
        [Export]
        public float Lifespan;
        [Export]
        public float ShotSpread;

        private Vector3 Velocity = Vector3.Zero;
        private Vector3 ProjectileGravity = Vector3.Down * 5;
        private float TimeAlive;
        private int HitsLeft;

        public override void _Ready()
        {
            Vector3 directionVector = -Transform.Basis.Z;

            Random rand = new Random();
            directionVector = directionVector.Rotated(Transform.Basis.X, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));
            directionVector = directionVector.Rotated(Transform.Basis.Y, ((float)rand.NextDouble() * ShotSpread*2) + (-ShotSpread));

            Velocity = directionVector * InitialVelocity;
        }

        public override void _Process(double delta)
        {
            TimeAlive += (float)delta;
            if (TimeAlive > Lifespan)
            {
                QueueFree();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            Velocity += ProjectileGravity * (float)delta;

            if (Velocity != Vector3.Zero)
            {
                LookAt(Transform.Origin + Velocity.Normalized(), Vector3.Up);
                Transform = Transform with { Origin = Transform.Origin + Velocity * (float)delta };

            }
        }

    }
}
