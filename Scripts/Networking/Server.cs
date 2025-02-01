using Godot;
using System;

public partial class Server : Node
{
    public static Server Instance { get; private set; }
    
    // Define the maximum number of clients allowed
    private const int MaxClients = 10;
    private string levelFilepath;
    private ENetMultiplayerPeer Peer;
    private bool AcceptingConnections { get; set; } = true; // whether or not the server is accepting connections or not


    // Handle disconnects
    public override void _ExitTree()
    {
        Peer.Close();
        base._ExitTree();
    }

    public override void _Ready()
    {   
        Instance = this;

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;

        Peer = new ENetMultiplayerPeer();
        Error err = Peer.CreateServer(3333, MaxClients);

        if (err != Error.Ok)
        {
            GD.PrintErr("Server._Ready - Error starting the server: " + err);
        }
        else
        {
            GD.Print("Server._Ready - Server started successfully.");
            Multiplayer.MultiplayerPeer = Peer;
        }
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Server.PeerDisconnected - Player disconnected: " + id);
        
        GameManager.RemovePlayer(id);
    }

    private void PeerConnected(long id) {
        GD.Print("Server.PeerConnected - Player connected: " + id);

        if (GameManager.GameHost == -1) { // First peer connected becomes host
            GameManager.GameHost = id;
        }
        
        RpcId(id, MethodName.NotifyCurrentHost, GameManager.GameHost);
        
        if(AcceptingConnections == false)
        {
            RpcId(id, MethodName.NotifyConnectionRefused, "Connection failed: Game has already started");
        }
    }

    /// <summary>
    /// Client uses this to tell the server about themselves
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerInfo(long id, string name) {
        if(AcceptingConnections)
        {
            Rpc(MethodName.NotifyPlayerConnected, id, name, GameManager.Players.Count);
        }
    }

    /// <summary>
    /// Server uses this to tell all clients about a new player that connected. Tells the client who connected about all previous connections
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerConnected(long id, string name, int sequenceNumber) {
        // For the peer that connected, we have to tell them about all other players that joined before them
        foreach(long peerId in GameManager.Players.Keys)
        {
            RpcId(id, MethodName.NotifyPlayerConnected, peerId, GameManager.Players[peerId].name, GameManager.Players[peerId].sequenceNumber);
        }

        GameManager.Players.Add(id, new GameManager.PlayerInfo{ name = name, sequenceNumber = sequenceNumber });
    }

    /// <summary>
    /// Server uses this to tell all clients to start their games
    /// </summary>
    /// <param name="filepath"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame(string filepath) {
        if(Multiplayer.GetRemoteSenderId() == GameManager.GameHost)
        {
            GD.Print("Server.NotifyStartGame - Telling clients to start their games");
            AcceptingConnections = false;
            Rpc(MethodName.NotifyStartGame, filepath);

            PackedScene levelPackedScene = (PackedScene)ResourceLoader.Load(filepath);
            Node level = levelPackedScene.Instantiate();
            Root.Instance.AddChild(level); // using Root.Instance just to access tree
            GameManager.CurrentLevel = level;
        }
    }

    /// <summary>
    /// Server uses this to tell all clients who the current host is
    /// </summary>
    /// <param name="hostId"></param>
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyCurrentHost(long hostId) {}

    /// <summary>
    /// Server uses this to tell a client that the connection was refused and why
    /// </summary>
    /// <param name="reason"></param>
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyConnectionRefused(string reason) {}
}
