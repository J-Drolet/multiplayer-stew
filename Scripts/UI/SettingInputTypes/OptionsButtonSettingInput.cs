using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class OptionsButtonSettingInput: SettingInput
{
    [Export, ExportRequired]
    public OptionButton Input { get; set; }

    public override void InitSetting() 
    {
        Input.Select(Input.GetItemIndex((int) Config.GetValue(SettingSection, SettingName)));
    }

    public override void SaveSetting() 
    {
        Config.SetValue(SettingSection, SettingName, Input.GetItemId(Input.Selected));
    }
}
