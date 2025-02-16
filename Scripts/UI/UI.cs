using Godot;
using System;

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
    public static GunViewCamera gunViewCamera { get; set; }
    
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
                PauseMenu.ToggleView();
            }

            if(ServerBrowser.Visible) 
            {
                ServerBrowser.Hide();
            }
        }
    }

    public static void ToggleSpinner(bool visibility) 
    {
        if(visibility) 
        {
            Spinner.Show();
        }
        else
        {
            Spinner.Hide();
        }
    }
}
