using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


namespace multiplayerstew.Scripts.Base
{
    public partial class SelfDestroyingParticle : Node3D
    {
        public enum SpawnMode {
            Local,
            Global
        }

        [Export, ExportRequired]
        public PackedScene ParticleScene {  get; set; }
        [Export, ExportRequired]
        public SpawnMode ParticleSpawnMode = SpawnMode.Global;

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
            else
            {
                SpawnParticles(1);
            }
        }

        public void EmitParticlesOnLocalOnlyLayer(int localLayer)
        {
            Rpc(MethodName.EmitParticles);
            SpawnParticles(localLayer);
        }

        private void SpawnParticles(int visibilityLayer)
        {
            GpuParticles3D particle3DInstance = ParticleScene.Instantiate() as GpuParticles3D;
            particle3DInstance.SetLayerMaskValue(1, false);
            particle3DInstance.SetLayerMaskValue(visibilityLayer, true);

            switch(ParticleSpawnMode)
            {
                case SpawnMode.Global:
                    LevelManager.Instance.AddChild(particle3DInstance);
                    break;
                case SpawnMode.Local:
                    AddChild(particle3DInstance);
                    break;
            }
            particle3DInstance.GlobalPosition = GlobalPosition;
            particle3DInstance.GlobalRotation = GlobalRotation;
            particle3DInstance.Emitting = true;
            particle3DInstance.Finished += particle3DInstance.QueueFree;
        }
    }
}
