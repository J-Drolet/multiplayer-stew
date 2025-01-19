using Godot;
using System;

public partial class Server : Node
{
    // Define the maximum number of clients allowed
    private const int MaxClients = 10;
    private ENetMultiplayerPeer _server;

    private bool AcceptingConnections = true; // whether or not the server is accepting connections or not

    public override void _Ready()
    {
        _server = new ENetMultiplayerPeer();
        Error err = _server.CreateServer(3333, MaxClients);

        if (err != Error.Ok)
        {
            GD.PrintErr("Error starting the server: " + err);
        }
        else
        {
            GD.Print("Server started successfully.");
            Multiplayer.MultiplayerPeer = _server;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceivePlayerInformation(string playerName, int id) 
    {
        if (AcceptingConnections == false) return; // if we aren't accepting connections then we don't add new players
	
        if (!GameManager.Players.ContainsKey(id)) 
        {
            GameManager.Players[id] = new GameManager.PlayerInfo
            {
                name = playerName,
                id = id
            };
        }

        /*
        foreach(int i in GameManager.Players.Keys)
        {
            Rpc
            rpc("send_player_info", GameManager.Players[i].name, GameManager.Players[i].color, i)
        }
        */
            
        
        //main.update_player_list()
    }

}
