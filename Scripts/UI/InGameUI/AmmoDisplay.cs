using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class AmmoDisplay : Control
{
    [Export, ExportRequired]
    public Texture2D UnfiredAmmoTexture;
    [Export, ExportRequired]
    public Texture2D FiredAmmoTexture;
    [Export, ExportRequired]
    public VBoxContainer AmmoContainer;
    [Export, ExportRequired]
    public MarginContainer MarginContainer;

    private float maxHeight;

    public override void _Ready() {
        GodotErrorService.ValidateRequiredData(this);
        maxHeight = Size.Y;
    }

    public void SetAmmo(int unfiredAmmo, int maxAmmo)
    {
        foreach(Node child in AmmoContainer.GetChildren())
        {
            child.QueueFree();
        }
        AmmoContainer.SetSize(new Vector2(Size.X, 0));
        AmmoContainer.SetPosition(Vector2.Zero);

        for(int i = 0; i < maxAmmo; i++)
        {
            TextureRect textureRect = new();
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

            AmmoContainer.AddChild(textureRect);
            Texture2D ammoTexture;
            if(i < (maxAmmo - unfiredAmmo)) 
            {
                ammoTexture = FiredAmmoTexture;
            }
            else
            {
                ammoTexture = UnfiredAmmoTexture;
            }
            textureRect.Texture = ammoTexture;
        }
        CustomMinimumSize = new Vector2(Size.X, AmmoContainer.Size.Y + MarginContainer.GetThemeConstant("margin_top") + MarginContainer.GetThemeConstant("margin_bottom"));
    }
}
