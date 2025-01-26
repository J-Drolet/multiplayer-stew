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
        string settingValue = Settings.GetValue(SettingName);
        
        if (settingValue == "true")
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
        string settingValue = isToggled ? "true" : "false";

        Settings.AddValue(SettingName, settingValue);
    }


}
