using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class JoinInformation : Control
{
    [Signal]
    public delegate void ConnectionRequestEventHandler(string ip, int port);

    [Export, ExportRequired]
    LineEdit IPInput;

    [Export, ExportRequired]
    LineEdit PortInput;

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
        EmitSignal("ConnectionRequest", IPInput.Text, PortInput.Text);
        Hide();
    }
}
