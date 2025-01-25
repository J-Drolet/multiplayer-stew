using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class SliderSettingInput: SettingInput
{
    [Export, ExportRequired]
    public Slider Input { get; set; }

    public override void InitSetting() 
    {
        Input.Value = Settings.GetValue(SettingName).ToFloat();
    }

    public override void SaveSetting() 
    {
        Settings.AddValue(SettingName, Input.Value.ToString());
    }
}
