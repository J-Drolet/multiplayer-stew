using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public partial class Client : Node
{
    public static Client Instance { get; private set; }

    private readonly string LocalHost = "127.0.0.1";
    private const int ServerPort = 3333;
    private ENetMultiplayerPeer _client;
    private int ServerPid;

    // Kill server process
    public override void _ExitTree()
    {
        OS.Kill(ServerPid);
        base._ExitTree();
    }

    public override void _Ready()
    {   
        Instance = this;

        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;

    }

    // Called when the "Create Lobby" button is pressed
    public void CreateLobbyAndConnect()
    {
        StartServer();
        ConnectToServer(LocalHost, ServerPort);  // Connect the client to the server
    }

    private void StartServer()
    {
        if (OS.HasFeature("editor")) // different execution chains for when it is packaged and when it is being tested in editor
        { 
            string[] args = new string[] { "--server" };//{ "--headless", "Scenes/server.tscn" };
            ServerPid = OS.CreateInstance(args);

            if(ServerPid != -1) 
            {
                GD.Print("Started the server in headless mode.");
            }
            else {
                GD.Print("Failed to start server instance.");
            }
        }
        else
        {
            GD.Print("Running in a build");
        }
    }

    private void CloseServer()
    {
        OS.Kill(ServerPid);
    }

    public void Disconnect()
    {
        if (GameManager.GameHost == Multiplayer.GetUniqueId())
        {
            CloseServer();
        }

        GameManager.Players.Clear();
        _client.Close();
    }

    public void StartGame() {
        RpcId(1, MethodName.NotifyStartGame);
    }

    public void ConnectToServer(string ip, int port)
    {
        // Set up the client network connection to the server
        _client = new ENetMultiplayerPeer();
        Error err = _client.CreateClient(ip, port);
        if(err != Error.Ok) {
            GD.Print("Client error: " + err);
        }
    
        Multiplayer.MultiplayerPeer = _client;
    }

    // called only on clients
    private void ConnectedToServer()
    {
        GD.Print("Connected to server");
        RpcId(1, MethodName.SendPlayerInfo, Multiplayer.GetUniqueId(), $"Player {Multiplayer.GetUniqueId()}");
        //main.connection_succeeded()
        //send_player_info.rpc_id(1, peer_name, peer_color, multiplayer.get_unique_id()
    }

    // called only on clients
    private void ConnectionFailed()
    {
        GD.Print("Failed Connection");
    }

    // this gets called on the server and client when someone connects
    private void PeerConnected(long id)
    {
        GD.Print($"Peer {Multiplayer.GetUniqueId()} received message: Player connected: {id}");
    }

    // this gets called on the server and client when someone disconnects
    private void PeerDisconnected(long id)
    {
        GD.Print($"Peer {Multiplayer.GetUniqueId()} received message: Player disconnected: {id}");
        GameManager.Players.Remove(id);
        if (id == 1) // if the server disconnected we get kicked back to main menu
        {
            //@TODO 
            //disconnect_from_server()
            //main.server_closed()
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerInfo(long id, string name) {}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerConnected(long id, string name) {
        GameManager.Players.Add(id, new GameManager.PlayerInfo{ name = name, id = id });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerDisconnected(long id) {
        GameManager.Players.Remove(id);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame() 
    {
        PackedScene levelPackedScene = (PackedScene)ResourceLoader.Load("res://Scenes/Level.tscn");
        Node level = levelPackedScene.Instantiate();
        GetTree().Root.AddChild(level);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyCurrentHost(long hostId) {
        GameManager.GameHost = hostId;
    }
}
