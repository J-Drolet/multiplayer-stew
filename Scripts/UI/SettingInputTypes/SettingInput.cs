using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public abstract partial class SettingInput : Node
{
    [Export, ExportRequired]
    public string SettingName { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        InitSetting();
    }

    public abstract void InitSetting();
    public abstract void SaveSetting();
}
