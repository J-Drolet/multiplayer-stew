using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class LineEditSettingInput: SettingInput
{
    [Export, ExportRequired]
    public LineEdit Input { get; set; }

    public override void InitSetting() 
    {
        Input.Text = Settings.GetValue(SettingName);
    }

    public override void SaveSetting() 
    {
        Settings.AddValue(SettingName, Input.Text);
    }
}
