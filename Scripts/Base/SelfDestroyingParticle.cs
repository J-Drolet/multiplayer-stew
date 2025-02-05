using Godot;
using multiplayerstew.Scripts.Attributes;
using System;


namespace multiplayerstew.Scripts.Base
{
    public partial class SelfDestroyingParticle : Node3D
    {
        [Export, ExportRequired]
        public PackedScene particleScene {  get; set; }

        public void emitParticles()
        {
            GpuParticles3D particle3DInstance = particleScene.Instantiate() as GpuParticles3D;
            AddChild(particle3DInstance);
            particle3DInstance.Ready += () => particle3DInstance.Emitting = true;
            particle3DInstance.Finished += particle3DInstance.QueueFree; 
        }
    }
}
