using Godot;
using System;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        UI.PauseMenu = this;
        VisibilityChanged += OnVisibilityChanged;
    }

    private void OnVisibilityChanged()
    {
        if(Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanMove = false;
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanLook = false;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanMove = true;
            GameManager.Players[Multiplayer.GetUniqueId()].characterNode.CanLook = true;
        }
    }

    public void OnReturnButtonPressed()
    {
        Hide();
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
