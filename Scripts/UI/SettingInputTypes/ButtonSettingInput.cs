using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class ButtonSettingInput: SettingInput
{
    [Export, ExportRequired]
    public Button Input { get; set; }

    public override void _Ready()
    {
        Input.ToggleMode = true;
    }

    public override void InitSetting() 
    {
        bool settingValue = (bool)Config.GetValue(SettingSection, SettingName);
        
        if (settingValue)
        {
            Input.ButtonPressed = true;
        }
        else
        {
            Input.ButtonPressed = false;
        }
    }

    public override void SaveSetting() 
    {
        bool isToggled = Input.ButtonPressed;

        Config.SetValue(SettingSection, SettingName, isToggled);
    }


}
