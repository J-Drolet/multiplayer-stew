using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class ServerUiElement : Control
{
    [Export, ExportRequired]
    public Label ServerName;
    [Export, ExportRequired]
    public Label IpAddress;
    [Export, ExportRequired]
    public Label PortNumber;
    [Export, ExportRequired]
    public Button JoinButton;
    [Export, ExportRequired]
    public Button DeleteButton;
}
