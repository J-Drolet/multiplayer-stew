using Godot;
using multiplayerstew.Scripts.Attributes;
using System;
using System.Runtime.CompilerServices;

public partial class OptionsButtonSettingInput: SettingInput
{
    [Export, ExportRequired]
    public OptionButton Input { get; set; }
    /// <summary>
    /// Whether or not to use the entry index in saving to Config. If true it will save the index which is good for static options. If it is false it requires manually setting the metadata
    /// </summary>
    [Export, ExportRequired]
    public bool UseIndexSaving { get; set; } = true; 

    public override void InitSetting() 
    {
        if(UseIndexSaving)
        {
            Input.Select(Input.GetItemIndex((int) Config.GetValue(SettingSection, SettingName)));
        }
        else
        {
            Input.Select(GetItemIndexWithSaveValue((string) Config.GetValue(SettingSection, SettingName)));
        }
    }

    public override void SaveSetting() 
    {
        if(UseIndexSaving)
        {
            Config.SetValue(SettingSection, SettingName, Input.GetItemId(Input.Selected));
        }
        else
        {
            Config.SetValue(SettingSection, SettingName, Input.GetItemMetadata(Input.Selected).ToString());
        }
    }
    
    private int GetItemIndexWithSaveValue(string saveValue)
    {
        for(int i = 0; i < Input.GetItemCount(); i++)
        {
            if(Input.GetItemMetadata(i).ToString() == saveValue)
            {
                return i;
            }
        }

        return 0;
    }
}
