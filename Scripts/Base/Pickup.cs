using Godot;
using multiplayerstew.Scripts.Attributes;
using System;


namespace multiplayerstew.Scripts.Base
{
    public abstract partial class Pickup: Node3D
    {
        [Export, ExportRequired]
        public Area3D PickupArea { get; set; }

        public override void _Ready()
        {
            PickupArea.BodyEntered += ActivatePickup;
        }

        protected abstract void ActivatePickup(Node3D body);

    }
}
