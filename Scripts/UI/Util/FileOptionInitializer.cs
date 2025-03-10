using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Linq;

public partial class FileOptionInitializer : OptionButton
{
    [Export, ExportRequired]
    public string ParentFilepath { get; set; }

    public override void _EnterTree()
    {
        GodotErrorService.ValidateRequiredData(this);

        int id;
        foreach(string filepath in GodotSceneFindingService.GetScenesAtFilepath(ParentFilepath)) 
        {
            id = GetItemCount() + 1;
            AddItem(filepath.Split('/').Last().Split('.').First(), id);
            SetItemMetadata(GetItemIndex(id), filepath);
        }
    }
}
