using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PowerPackDisplayManager : Node3D
{
    [Export, ExportRequired]
    public PackedScene PackScene { get; set; }
    private List<PowerPackDisplay> Packs = [];
    private int LastPowerLevel = 0;
    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        // Create 1st Pack
        PowerPackDisplay newPack = PackScene.Instantiate<PowerPackDisplay>();
        AddChild(newPack);
        Packs.Add(newPack);
    }
    public override void _Process(double delta)
    {
        int? newPowerLevel = LevelManager.Instance?.LevelPeerInfo.GetValueOrDefault(Multiplayer.GetUniqueId())?.characterNode?.CalculatePowerLevel();
        if (newPowerLevel != null)
        {
            try
            {
                // Adding Cigs
                if (newPowerLevel.Value > LastPowerLevel)
                {
                    int cigsToAdd = newPowerLevel.Value - LastPowerLevel;
                    int latestPack = Packs.Count - 1;
                    while (cigsToAdd > 0)
                    {
                        cigsToAdd = Packs[latestPack].FillPack(cigsToAdd);
                        if (cigsToAdd > 0)
                        {
                            //Add pack
                            PowerPackDisplay newPack = PackScene.Instantiate<PowerPackDisplay>();
                            AddChild(newPack);
                            Packs.Add(newPack);
                            // Move new pack above the old one
                            newPack.GlobalTransform = Packs[latestPack].PackMeshes.GlobalTransform;
                            newPack.Translate(new Vector3(0, 1.0f, 0));
                            // Spawn meshes left that are animating from the left so they don't cause a flicker effect
                            newPack.PackMeshes.Translate(new Vector3(-5.0f, 0, 0));
                            newPack.APlayer.Play("Spawn");

                            latestPack++;
                        }
                    }
                }
                // Removing Cigs
                else if (newPowerLevel.Value < LastPowerLevel)
                {
                    int cigsToRemove = LastPowerLevel - newPowerLevel.Value;
                    for (int x = LastPowerLevel - 1; cigsToRemove > 0; x--)
                    {
                        PowerPackDisplay currentPack = Packs.Last();
                        cigsToRemove = currentPack.EmptyPack(cigsToRemove);
                        // Remove empty packs (Always keep 1st one)
                        if (x > 0 && currentPack.NumberOfCigs <= 0)
                        {
                            currentPack.APlayer.Play("Despawn");
                            Packs.Remove(currentPack);
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                GD.PrintErr("Power pack display had an exception");
            }
            LastPowerLevel = newPowerLevel.Value;
        }
    }
}
