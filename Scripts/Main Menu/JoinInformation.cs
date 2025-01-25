using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class JoinInformation : Control
{
    [Export, ExportRequired]
    LineEdit IPInput { get; set; }

    [Export, ExportRequired]
    LineEdit PortInput { get; set; }

    [Export, ExportRequired]
    Control LobbyPage { get; set; }

   public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
    }


    public void OnCancelPressed() 
    {
        Hide();
    }

    public void OnConnectPressed() 
    {   
        Client.Instance.ConnectToServer(IPInput.Text, int.Parse(PortInput.Text));
        Hide();
        LobbyPage.Show();
        UI.ToggleSpinner(true);
    }
}
