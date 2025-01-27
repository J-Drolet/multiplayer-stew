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
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanMove = false;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanMove = true;
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
    }

    public void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

}
