using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SettingsScreen : Control
{
    [Export, ExportRequired]
    public Control GameTab;
    [Export, ExportRequired]
    public Control ControlTab;
    [Export, ExportRequired]
    public Control VideoTab;
    [Export, ExportRequired]
    public Control AudioTab;

    private List<SettingInput> SettingInputs;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.SettingsScreen = this;
        SettingInputs = GodotNodeFindingService.FindNodes<SettingInput>(this);
    }

    public void OnVisibilityChanged()
    {
        if (Visible) // On opening of the settings screen we have to update the GUI to reflect the Settings datasctructure (in case settings were changed somewhere else)
        {
            OnGameButtonPressed(); // reset back to the first tab
            ResetSettingsInputFromConfig();
        }
    }

    private void ResetSettingsInputFromConfig()
    {
        foreach(SettingInput settingInput in SettingInputs)
        {
            settingInput.InitSetting();
        }

    }

    public void HideAllTabs()
    {
        GameTab.Visible = false;
        ControlTab.Visible = false;
        VideoTab.Visible = false;
        AudioTab.Visible = false;
    }

    public void OnBackButtonPressed()
    {
        Hide();
    }

    public void OnGameButtonPressed()
    {
        HideAllTabs();
        GameTab.Visible = true;
    }

    public void OnControlButtonPressed()
    {
        HideAllTabs();
        ControlTab.Visible = true;
    }

    public void OnVideoButtonPressed()
    {
        HideAllTabs();
        VideoTab.Visible = true;
    }

    public void OnAudioButtonPressed()
    {
        HideAllTabs();
        AudioTab.Visible = true;
    }

    public void OnSaveButtonPressed()
    {
        foreach (var settingInput in SettingInputs)
        {
            settingInput.SaveSetting();
        }
    }

    public void OnRestoreDefaultsButtonPressed()
    {   
        // loop through each setting and set back to default if it is tagged as resettable
        foreach(SettingInput settingInput in SettingInputs)
        {
            if(settingInput.ResettableToDefault)
            {
                Config.RestoreDefault(settingInput.SettingSection, settingInput.SettingName);
            }
        }
        ResetSettingsInputFromConfig();
    }
}
