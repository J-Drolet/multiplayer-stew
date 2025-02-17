using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class EndOfGame : Control
{
    [Export]
    public double TimeTillNextGame;
    [Export, ExportRequired]
    public Button NextGameButton;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        UI.EndOfGame = this;
        VisibilityChanged += OnVisibilityChanged;
    }

    public override void _Process(double delta)
    {
        TimeTillNextGame -= delta;
        NextGameButton.Text = $"Next Game in {Math.Clamp((int)TimeTillNextGame, 0, int.MaxValue)}...";
    }

    private void OnVisibilityChanged()
    {
        if(Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            UI.Scoreboard.Show();
            TimeTillNextGame = (double)Config.GetValue("game_constants", "seconds_between_games", true);
        }
    }

    public void OnLeaveButtonPressed()
    {
        GameManager.LeaveJoinedGame();
    }
}
