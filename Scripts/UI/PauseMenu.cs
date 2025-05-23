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
            if(LevelManager.Instance != null && LevelManager.Instance.LevelPeerInfo.ContainsKey(Multiplayer.GetUniqueId()) && LevelManager.Instance.LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode != null)
            {
                Character localCharacter = LevelManager.Instance.LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode;
                localCharacter.CanMove.AddLock("PauseMenu");
                localCharacter.CanLook.AddLock("PauseMenu");
                localCharacter.CanFire.AddLock("PauseMenu");
            }
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            if(LevelManager.Instance != null && LevelManager.Instance.LevelPeerInfo.ContainsKey(Multiplayer.GetUniqueId()) && LevelManager.Instance.LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode != null)
            {
                Character localCharacter = LevelManager.Instance.LevelPeerInfo[Multiplayer.GetUniqueId()].characterNode;
                localCharacter.CanMove.RemoveLock("PauseMenu");
                localCharacter.CanLook.RemoveLock("PauseMenu");
                localCharacter.CanFire.RemoveLock("PauseMenu");
            }
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
        GameSessionManager.LeaveJoinedGame();
    }

    public void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

}
