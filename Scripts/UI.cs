using Godot;
using System;

public static class UI
{
    public static MainMenu MainMenu { get; set; }
    
    public static ErrorMessage ErrorMessage { get; set; }

    public static Control Spinner { get; set; }
    
    public static void DisplayError(string errorText) 
    {   
        ErrorMessage.DisplayError(errorText);
    }

    public static void ShowSpinner() 
    {
        Spinner.Show();
    }

    public static void HideSpinner() 
    {
        Spinner.Hide();
    }
}
