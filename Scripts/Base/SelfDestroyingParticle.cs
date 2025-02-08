using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;


namespace multiplayerstew.Scripts.Base
{
    public partial class SelfDestroyingParticle : Node3D
    {
        [Export, ExportRequired]
        public PackedScene ParticleScene {  get; set; }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void EmitParticles()
        {   
            if(Multiplayer.GetRemoteSenderId() == 0) 
            {
                Rpc(MethodName.EmitParticles); // tell other clients to spawn particles if not in a RPC
            }
            else // only spawn in a RPC
            {
                GpuParticles3D particle3DInstance = ParticleScene.Instantiate() as GpuParticles3D;
                AddChild(particle3DInstance);
                particle3DInstance.GlobalRotation = GlobalRotation;
                particle3DInstance.Emitting = true;
                particle3DInstance.Finished += particle3DInstance.QueueFree; 
            }
        }
    }
}
