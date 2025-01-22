using Godot;
using System;

public partial class Server : Node
{
    public static Server Instance { get; private set; }

    // Define the maximum number of clients allowed
    private const int MaxClients = 10;
    private ENetMultiplayerPeer _server;
    private bool AcceptingConnections { get; set; } = true; // whether or not the server is accepting connections or not


    // Handle disconnects
    public override void _ExitTree()
    {
        _server.Close();
        base._ExitTree();
    }

    public override void _Ready()
    {   
        Instance = this;

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;

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

    private void PeerDisconnected(long id)
    {
        GD.Print("Server received message: Player connected: " + id);
        
        GameManager.RemovePlayer(id);
    }

    // this gets called on the server and client when someone connects

    private void PeerConnected(long id) {
        GD.Print("Server received message: Player connected: " + id);

        if (GameManager.GameHost == -1) {
            GameManager.GameHost = id;
        }
        
        if (!AcceptingConnections == false)
        {
            // tell peer that the server is refusing the connection
            // @TODO 
            //rpc_id(id, "connection_refused")
            
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerInfo(long id, string name) {
        Rpc(MethodName.NotifyPlayerConnected, id, name);
        Rpc(MethodName.NotifyCurrentHost, GameManager.GameHost);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerConnected(long id, string name) {
        GameManager.Players.Add(id, new GameManager.PlayerInfo{ name = name, id = id });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerDisconnected(long id) {
        GameManager.Players.Remove(id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame() {
        GD.Print("Starting");
        Rpc(MethodName.NotifyStartGame);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyCurrentHost(long hostId) {}
}
