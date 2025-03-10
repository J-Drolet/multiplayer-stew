using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class AnimatedCig : Node3D
{
    [Export, ExportRequired, AnimationsRequired(["Add"])]
    public AnimationPlayer APlayer { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
    }

    public void Add()
    {
        APlayer.Play("Add");
    }
    public void Remove() 
    {
        APlayer.PlayBackwards("Add");
    }
}
