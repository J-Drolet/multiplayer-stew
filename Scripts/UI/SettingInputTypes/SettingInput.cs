using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public abstract partial class SettingInput : Node
{
    [Export, ExportRequired]
    public string SettingSection { get; set; }
    
    [Export, ExportRequired]
    public string SettingName { get; set; }

    [Export]
    public bool ResettableToDefault = true; // whether or not this setting input should be defaulted back to default. i.e. PlayerName does not make sense to reset to default


    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        InitSetting();
    }

    public abstract void InitSetting();
    public abstract void SaveSetting();
}
