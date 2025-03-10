using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Linq;

public partial class CharacterPreview : Control
{
    [Export, ExportRequired]
    public OptionButton HeadCosmeticOption { get; set; }
    [Export, ExportRequired]
    public OptionButton FaceCosmeticOption { get; set; }

    [Export, ExportRequired]
    public Node3D HeadSlot { get; set; }
    [Export, ExportRequired]
    public Node3D FaceSlot { get; set; }

    [Export]
    public AnimationPlayer APlayer { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        if(APlayer != null)
        {
            APlayer.Play("bob_camera");
        }

        HeadCosmeticOption.ItemSelected += OnHeadCosmeticOptionSelected;
        FaceCosmeticOption.ItemSelected += OnFaceCosmeticOptionSelected;

        OnHeadCosmeticOptionSelected(HeadCosmeticOption.Selected);
        OnFaceCosmeticOptionSelected(FaceCosmeticOption.Selected);
    }


    private void OnHeadCosmeticOptionSelected(long index)
    {
        if(index == -1) return;

        HeadSlot.GetChildren().ToList().ForEach(child => child.QueueFree());

        string filepath = HeadCosmeticOption.GetItemMetadata((int)index).ToString();
        if(filepath != null)
        {
            HeadSlot.AddChild(GD.Load<PackedScene>(filepath).Instantiate());
        }
    }
    
    private void OnFaceCosmeticOptionSelected(long index)
    {
        if(index == -1) return;

        FaceSlot.GetChildren().ToList().ForEach(child => child.QueueFree());

        string filepath = FaceCosmeticOption.GetItemMetadata((int)index).ToString();
        if(filepath != null)
        {
            FaceSlot.AddChild(GD.Load<PackedScene>(filepath).Instantiate());
        }
    }
}
