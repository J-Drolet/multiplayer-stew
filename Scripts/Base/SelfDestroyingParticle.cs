using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;


namespace multiplayerstew.Scripts.Base
{
    public partial class SelfDestroyingParticle : Node3D
    {
        [Export, ExportRequired]
        public PackedScene particleScene {  get; set; }

        public override void _Ready()
        {
            GodotErrorService.ValidateRequiredData(this);
        }

        public void emitParticles()
        {
            GpuParticles3D particle3DInstance = particleScene.Instantiate() as GpuParticles3D;
            AddChild(particle3DInstance);
            particle3DInstance.GlobalRotation = GlobalRotation;
            particle3DInstance.Ready += () => particle3DInstance.Emitting = true;
            particle3DInstance.Finished += particle3DInstance.QueueFree; 
        }
    }
}
