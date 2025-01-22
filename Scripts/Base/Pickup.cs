using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;


namespace multiplayerstew.Scripts.Base
{
    public abstract partial class Pickup: Node3D
    {
        [Export, ExportRequired]
        public Area3D PickupArea { get; set; }
        [Export]
        public AnimationPlayer APlayer { get; set; }

        public override void _Ready()
        {
            if (APlayer != null && APlayer.HasAnimation("PickupIdle"))
            {
                APlayer.Play("PickupIdle");
            }
            PickupArea.BodyEntered += ActivatePickup;
            GodotErrorService.ValidateRequiredData(this);
        }

        protected abstract void ActivatePickup(Node3D body);

    }
}
