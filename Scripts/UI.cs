using Godot;
using System;

public static class UI
{
    public static MainMenu MainMenu { get; set; }
    public static Lobby Lobby { get; set; }
    
    public static ErrorMessage ErrorMessage { get; set; }

    public static Control Spinner { get; set; }
    
    public static void DisplayError(string errorText) 
    {   
        ErrorMessage.DisplayError(errorText);
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
