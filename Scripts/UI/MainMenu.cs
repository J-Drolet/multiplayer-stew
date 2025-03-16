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
    public AudioStreamPlayer MainMenuMusic { get; set; }


    public override void _Ready()
    {
        UI.MainMenu = this;
        GodotErrorService.ValidateRequiredData(this);
        VisibilityChanged += OnVisibilityChanged;
    }

    private void OnVisibilityChanged()
    {
        CloseAllWindows();

        if(Visible)
        {
            MainMenuMusic.Play();
        }
        else
        {
            MainMenuMusic.Stop();
        }

    }

    public void CloseAllWindows() {
        JoinInformation.Hide();
        LobbyPage.Hide();
        UI.Scoreboard.Hide();
    }

    public void OnHostPressed()
    {
        CloseAllWindows();
        Client.Instance.CreateLobbyAndConnect();
        LobbyPage.Show();
        UI.Spinner.Show();
    }

    public void OnJoinPressed()
    {
        CloseAllWindows();
        JoinInformation.Show();
    }

    public void OnSettingsPressed() 
    {
        CloseAllWindows();
        UI.SettingsScreen.Show();
    }

    public void OnQuitPressed() 
    {
        GetTree().Quit();
    }
}
