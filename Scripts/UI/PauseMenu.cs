using Godot;
using System;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        UI.PauseMenu = this;
    }

    public void ToggleView()
    {
        Visible = !Visible;

        if(Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }


    public void OnReturnButtonPressed()
    {
        ToggleView();
    }

    public void OnSettingsButtonPressed()
    {
        UI.SettingsScreen.Visible = true;
    }

    public void OnLeaveButtonPressed()
    {
        GameManager.LeaveJoinedGame();
        UI.MainMenu.Show();
        UI.PauseMenu.Hide();
        UI.InGameUI.Hide();
    }

    public void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

}
