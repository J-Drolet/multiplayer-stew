using Godot;
using multiplayerstew.Scripts.Attributes;
using System;


public partial class Lobby : Control
{
    [Export, ExportRequired]
    public Control PlayerPrototype { get; set; } 

    public override void _Process(double delta)
    {
        foreach(Node child in PlayerPrototype.GetParent().GetChildren())
        {
            if(child != PlayerPrototype)
            {
                child.QueueFree();
            }
        }

        foreach(GameManager.PlayerInfo player in GameManager.Players.Values)
        {
            AddPlayer(player.id, player.name);
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
