using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Data.Common;


public partial class Lobby : Control
{
    [Export, ExportRequired]
    public Control PlayerPrototype { get; set; }
    
    [Export, ExportRequired]
    public Control StartGameButton { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.Lobby = this;
    }

    public override void _Process(double delta)
    {
        foreach(Node child in PlayerPrototype.GetParent().GetChildren())
        {
            if(child != PlayerPrototype)
            {
                child.QueueFree();
            }
        }

        foreach(long id in GameManager.Players.Keys)
        {
            AddPlayer(id, GameManager.Players[id].name);
        }
    }

    public void AddPlayer(long id, string name)
    {
        Control player = PlayerPrototype.Duplicate() as Control;
        player.GetNode<Label>("Name").Text = id.ToString();
        PlayerPrototype.GetParent().AddChild(player);
        player.Show();
    }

    public void OnLeavePressed() 
    {   
        Client.Instance.Disconnect();
        Hide();
    }

    public void OnStartPressed() 
    {
        Client.Instance.StartGame();
    }
}
