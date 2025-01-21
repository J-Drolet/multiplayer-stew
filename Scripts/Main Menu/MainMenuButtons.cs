using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class MainMenuButtons : Node
{
    [Export, ExportRequired]
    public Control JoinInformation { get; set; }

    [Export, ExportRequired]
    public Control HostInformation { get; set; }

    [Export, ExportRequired]
    public Control Settings { get; set; }

    public override void _Ready()
    {
        //GodotErrorService.ValidateRequiredData(this);
    }

    private void CloseAllWindows() {
        JoinInformation.Hide();
        //HostInformation.Hide();
        //Settings.Hide();
    }

    public void OnHostPressed()
    {
        CloseAllWindows();
        HostInformation.Show();
    }

    public void OnJoinPressed()
    {
        CloseAllWindows();
        JoinInformation.Show();

    }

    public void OnSettingsPressed() 
    {
        CloseAllWindows();
        Settings.Show();

    }

    public void OnQuitPressed() 
    {
        GetTree().Quit();
    }
}
