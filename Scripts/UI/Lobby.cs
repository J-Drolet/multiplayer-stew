using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Data.Common;
using System.Linq;


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

    public void RefreshLobby()
    {
        foreach(Node child in PlayerPrototype.GetParent().GetChildren())
        {
            if(child != PlayerPrototype)
            {
                child.QueueFree();
            }
        }

        foreach(long id in GameManager.Players.Keys.OrderBy(x => GameManager.Players[x].sequenceNumber))
        {
            AddPlayer(id, GameManager.Players[id].name);
        }
    }

    private void AddPlayer(long id, string name)
    {
        Control player = PlayerPrototype.Duplicate() as Control;
        string symbol = id == GameManager.GameHost ? "♔" : "";
        player.GetNode<Label>("Name").Text = $"{name} ({id}) Sequence: {GameManager.Players[id].sequenceNumber} {symbol}";
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
