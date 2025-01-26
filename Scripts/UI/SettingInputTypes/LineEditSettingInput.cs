using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class LineEditSettingInput: SettingInput
{
    [Export, ExportRequired]
    public LineEdit Input { get; set; }

    public override void InitSetting() 
    {
        Input.Text = Config.GetValue(SettingSection, SettingName).ToString();
    }

    public override void SaveSetting() 
    {
        Config.SetValue(SettingSection, SettingName, Input.Text);
    }
}
