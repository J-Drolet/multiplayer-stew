using Godot;
using Godot.Collections;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public abstract partial class TimedPickup: Node3D
    {
        [Export, ExportRequired]
        public Area3D PickupArea { get; set; }
        [Export]
        public float RespawnTime { get; set;} = 10; // in seconds
        [Export, ExportRequired]
        public Node3D MeshParent { get; set; } // the node where upgrade meshes and cooldown sprite will live
        [Export, ExportRequired]
        public Range ProgressBar { get; set; }
        [Export, ExportRequired]
        public Node3D PickupLight { get; set; }

        private bool Active = true;
        [Export]
        public double TimeSinceLastActive;
        private Node3D UpgradeMesh;
        protected RandomNumberGenerator rng;

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);

            rng = new();

            MultiplayerSpawner multiplayerSpawner = new();
            multiplayerSpawner.Name = "Spawner";
            MeshParent.AddChild(multiplayerSpawner);
            multiplayerSpawner.SpawnPath = MeshParent.GetPath();

            foreach(string path in GetSpawnPaths())
            {
                multiplayerSpawner.AddSpawnableScene(ProjectSettings.LocalizePath(path));
            }

            if(!Multiplayer.IsServer()) return; // pickups are handled server-side
            PickupArea.BodyEntered += PickupAreaEntered;

            RespawnPickup();
        }

        public override void _Process(double delta)
        {
            if(!IsMultiplayerAuthority()) 
            {
                if(ProgressBar != null)
                {
                    ProgressBar.Value = TimeSinceLastActive / RespawnTime;
                }

                return;
            }

            TimeSinceLastActive += delta;

            if(!Active && TimeSinceLastActive > RespawnTime)
            {
                ProgressBar.Hide();
                Active = true;
                
                RespawnPickup();
            }

            PickupLight.Visible = Active;
        }

        protected abstract void ActivatePickup(Character character);

        protected abstract Array<string> GetSpawnPaths();

        protected virtual PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {
            return null;
        }

        protected virtual bool CanPickup(Character character) // marked virtual so I can put it in _Process
        {
            return true;
        }

        private void RespawnPickup()
        {
            UpgradeMesh = (Node3D)GetRespawn().Instantiate();
            MeshParent.AddChild(UpgradeMesh, true);
            UpgradeMesh.GlobalPosition = MeshParent.GlobalPosition;
        }

        private void PickupAreaEntered(Node3D body) 
        {   
            if(Active)
            {
                if(body is Character character)
                {
                    if(CanPickup(character))
                    {
                        ActivatePickup(character);
                        TimeSinceLastActive = 0;
                        Active = false;
                        ProgressBar.Show();

                        UpgradeMesh.QueueFree();
                        UpgradeMesh = null;
                    }
                }  
            }
        }
    }
}
