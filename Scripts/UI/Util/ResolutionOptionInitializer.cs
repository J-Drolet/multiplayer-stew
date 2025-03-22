using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Linq;

public partial class ResolutionOptionInitializer : OptionButton
{
    public class ResolutionOption
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ResolutionOption(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }

    private static ResolutionOption[] SupportedResolutions = 
    {
        new ResolutionOption(640, 360),
        new ResolutionOption(1280, 720),
        new ResolutionOption(1920, 1080),
        new ResolutionOption(2560, 1440),
        new ResolutionOption(3840, 2160)
    };

    public override void _EnterTree()
    {
        GodotErrorService.ValidateRequiredData(this);

        int id;
        foreach(ResolutionOption resolution in SupportedResolutions) 
        {
            id = GetItemCount() + 1;
            AddItem(resolution.ToString(), id);
            SetItemMetadata(GetItemIndex(id), resolution.ToString());
        }
    }
}
