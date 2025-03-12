using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ItemDisplay : MarginContainer
{
    [Export, ExportRequired]
    public GridContainer ItemGrid { get; set; }

    private Dictionary<Upgrade, CompressedTexture2D> Thumbnails { get; set; } = [];

    private PlaceholderTexture2D dummyTexture = new() { Size = new Vector2(64.0f, 64.0f) }; // Dummy texture for missing thumbnails

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        LoadThumbnails();
    }
    public void Refresh(IEnumerable<Upgrade> upgrades)
    {
        // Empty Item Grid
        foreach(Node child in ItemGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Fill Item Grid
        foreach (Upgrade upgrade in upgrades) 
        {
            TextureRect thumbnail = new();
            thumbnail.Texture = Thumbnails.ContainsKey(upgrade) ? Thumbnails[upgrade] : dummyTexture;
            ItemGrid.AddChild(thumbnail);
        }
    }

    private void LoadThumbnails()
    {
        foreach(string thumbnailPath in GodotFileFindingService.GetFilesAtFilePath(Root.ThumbnailFilepath, ["png"]))
        {
            CompressedTexture2D thumbnail = ResourceLoader.Load<CompressedTexture2D>(thumbnailPath);
            Upgrade thumbnailUpgrade = (Upgrade)Enum.Parse(typeof(Upgrade), GodotFileFindingService.GetFileName(thumbnailPath));
            Thumbnails.Add(thumbnailUpgrade, thumbnail);
        }
    }
}
