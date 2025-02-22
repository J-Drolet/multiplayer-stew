using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class AmmoLabel : Control
{
    [Export, ExportRequired]
    public Texture2D UnfiredAmmoTexture;
    [Export, ExportRequired]
    public Texture2D FiredAmmoTexture;
    private float maxHeight;

    public override void _Ready() {
        GodotErrorService.ValidateRequiredData(this);
        maxHeight = Size.Y;
    }

    public void SetAmmo(int unfiredAmmo, int maxAmmo)
    {
        foreach(Node child in GetChildren())
        {
            child.QueueFree();
        }

        float textureWidth = CustomMinimumSize.X;
        float textureAspectRatio = UnfiredAmmoTexture.GetSize().Y / UnfiredAmmoTexture.GetSize().X; // assume unfired and fired have same aspect ratio
        float textureHeight = textureWidth * textureAspectRatio;
        
        if(maxAmmo * textureHeight > maxHeight) 
        {
            textureHeight = maxHeight / maxAmmo;
            textureWidth = textureHeight / textureAspectRatio;
        }
        CustomMinimumSize = new Vector2(Size.X, textureHeight * maxAmmo);

        for(int i = 0; i < maxAmmo; i++)
        {
            TextureRect textureRect = new();
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

            AddChild(textureRect);
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

            textureRect.SetSize(new Vector2(textureWidth, textureHeight));
            textureRect.SetPosition(new Vector2(Size.X - textureWidth, textureHeight * i));
        }
    }
}
