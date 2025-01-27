using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class SliderSettingInput: SettingInput
{
    [Export, ExportRequired]
    public Slider Input { get; set; }

    public override void InitSetting() 
    {
        Input.Value = (double)Config.GetValue(SettingSection, SettingName);
    }

    public override void SaveSetting() 
    {
        Config.SetValue(SettingSection, SettingName, Input.Value);
    }
}
