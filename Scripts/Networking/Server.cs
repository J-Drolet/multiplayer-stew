using Godot;
using System;

public partial class Server : Node
{
    // Define the maximum number of clients allowed
    private const int MaxClients = 10;
    private ENetMultiplayerPeer _server;

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

    }

}
