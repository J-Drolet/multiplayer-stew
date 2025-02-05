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

        [Export]
        public bool DestroyOnPickup { get; set; } = true;

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
            if (APlayer != null && APlayer.HasAnimation("PickupIdle"))
            {
                APlayer.Play("PickupIdle");
            }

            if(!Multiplayer.IsServer()) return; // pickups are handled server-side
            PickupArea.BodyEntered += PickupAreaEntered;
        }

        protected abstract void ActivatePickup(Character character);

        private void PickupAreaEntered(Node3D body) 
        {
            if(body is Character)
            {
                Character character = body as Character;
                ActivatePickup(character);
                if(DestroyOnPickup) 
                {
                    Rpc(MethodName.DestroySelf);
                }
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void DestroySelf()
        {
            QueueFree();
        }
    }
}
