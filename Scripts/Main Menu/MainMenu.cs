using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class MainMenu : Control
{
    [Export, ExportRequired]
    public Control JoinInformation { get; set; }

    [Export, ExportRequired]
    public Control LobbyPage { get; set; }

    [Export, ExportRequired]
    public Control Settings { get; set; }


    public override void _Ready()
    {
        UI.MainMenu = this;
        GodotErrorService.ValidateRequiredData(this);
    }

    public void CloseAllWindows() {
        JoinInformation.Hide();
        LobbyPage.Hide();
        //Settings.Hide();
    }

    public void OnHostPressed()
    {
        CloseAllWindows();
        Client.Instance.CreateLobbyAndConnect();
        LobbyPage.Show();
        UI.ToggleSpinner(true);
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
