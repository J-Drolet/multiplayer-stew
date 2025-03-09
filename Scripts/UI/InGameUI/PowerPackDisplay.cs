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
    [Export, ExportRequired]
    public Node3D PackMeshes {  get; set; }

    [Export, ExportRequired, AnimationsRequired(["Close", "Open", "Spawn", "Despawn"])]
    public AnimationPlayer APlayer { get; set; }
    private List<AnimatedCig> animatedCigs = [];
    public int NumberOfCigs { get; private set; } = 0;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        animatedCigs = CigNode.GetChildren().Select(c => c as AnimatedCig).OrderByDescending(c => c.Name.ToString().ToInt()).ToList();
    }

    public int FillPack(int amount)
    {
        int remainder = Math.Clamp(amount - (20 - NumberOfCigs), 0, int.MaxValue);
        amount -= remainder;
        int addToIndex = amount + NumberOfCigs;
        for (int x = NumberOfCigs; x < addToIndex; x++) 
        {
            NumberOfCigs++;
            animatedCigs[x].Add();
        }

        // Pack is full. Close it
        if (remainder > 0) 
        {
            APlayer.Play("Close");
        }
        return remainder;
    }

    public int EmptyPack(int amount)
    {
        //Open Closed Pack
        if(NumberOfCigs == 20)
        {
            APlayer.Play("Open");
        }

        int remainder = Math.Clamp(amount - NumberOfCigs, 0, int.MaxValue);
        amount -= remainder;
        int removeToIndex = NumberOfCigs - amount;
        for (int x = NumberOfCigs - 1; x >= removeToIndex; x--)
        {
            NumberOfCigs--;
            animatedCigs[x].Remove();
        }
        return remainder;
    }
}
