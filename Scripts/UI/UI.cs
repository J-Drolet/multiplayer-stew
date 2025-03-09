using Godot;
using System;
using System.Collections.Generic;

public partial class UI: Node
{
    public static MainMenu MainMenu { get; set; }
    public static Lobby Lobby { get; set; }
    public static ServerBrowser ServerBrowser { get; set; }
    public static PauseMenu PauseMenu { get; set; }
    public static ErrorMessage ErrorMessage { get; set; }
    public static Control Spinner { get; set; }
    public static SettingsScreen SettingsScreen { get; set; }
    public static InGameUI InGameUI { get; set; }
    public static GunViewCamera GunViewCamera { get; set; }
    public static Scoreboard Scoreboard { get; set; }
    public static EndOfGame EndOfGame { get; set; }
    public static Hitmarker Hitmarker { get; set; }
    
    public static void DisplayError(string errorText) 
    {   
        ErrorMessage.DisplayError(errorText);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event.IsActionPressed("PauseMenu")) {
            if(SettingsScreen.Visible) 
            {
                SettingsScreen.Visible = false;
            }
            else if(!MainMenu.Visible) // if in game we can toggle the pause menu
            {
                PauseMenu.Visible = !PauseMenu.Visible;
            }

            if(ServerBrowser.Visible) 
            {
                ServerBrowser.Hide();
            }
        }
    }

    public override void _Process(double delta)
    {
        if(Multiplayer.IsServer()) return;
        
        if(MainMenu.Visible || (!Input.IsActionPressed("ViewScoreboard") && !EndOfGame.Visible))
        {
            Scoreboard.Hide();
        }
        else if(Input.IsActionJustPressed("ViewScoreboard"))
        {
            Scoreboard.Show();
        }

    }
}
