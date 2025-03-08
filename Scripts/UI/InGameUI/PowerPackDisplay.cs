using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PowerPackDisplay : Node3D
{
    [Export, ExportRequired]
    public Node3D CigNode { get; set; }

    private List<AnimatedCig> animatedCigs = [];

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        animatedCigs = CigNode.GetChildren().Select(c => c as AnimatedCig).OrderByDescending(c => c.Name).ToList();
    }




}
