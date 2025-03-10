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

    [Export, ExportRequired]
    public Control GameSettings { get; set; }

    [Export, ExportRequired]
    public OptionButton LevelSelector { get; set; }

    [Export, ExportRequired]
    public SpinBox MaxTimeInput { get; set; }

    [Export, ExportRequired]
    public SpinBox MaxAuraInput { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.Lobby = this;

        // init list of levels
        foreach(string filepath in GodotSceneFindingService.GetScenesAtFilepath(Root.LevelsFilepath)) 
		{
            LevelSelector.AddItem(filepath.Split('/').Last().Split('.').First());
        }
    }

    public void SetHost(long hostId)
    {
        if (hostId == Multiplayer.GetUniqueId()) {
            UI.Lobby.StartGameButton.Show();
            GameSettings.Show();
        }
        else 
        {
            UI.Lobby.StartGameButton.Hide();
            GameSettings.Hide();
        }
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

        foreach(long id in GameSessionManager.ConnectedPeers.Keys.OrderBy(x => GameSessionManager.ConnectedPeers[x].sequenceNumber))
        {
            AddPlayer(id, GameSessionManager.ConnectedPeers[id].name);
        }
    }

    private void AddPlayer(long id, string name)
    {
        Control player = PlayerPrototype.Duplicate() as Control;
        string symbol = id == GameSessionManager.GameHost ? "♔" : "";
        player.GetNode<Label>("Name").Text = $"{name} ({id}) Sequence: {GameSessionManager.ConnectedPeers[id].sequenceNumber} {symbol}";
        PlayerPrototype.GetParent().AddChild(player);
        player.Show();
    }

    public void OnLeavePressed() 
    {   
        Client.Instance.Disconnect();
        UI.Spinner.Hide();
        Hide();
    }

    public void OnStartPressed() 
    {
        Client.Instance.StartGame(Root.LevelsFilepath + LevelSelector.GetItemText(LevelSelector.Selected) + ".tscn", (int)MaxTimeInput.Value * 60, (int)MaxAuraInput.Value);
    }
}
