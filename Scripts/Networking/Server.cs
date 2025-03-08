using Godot;
using System;
using System.Collections.Generic;

public partial class Server : Node
{
    public static Server Instance { get; private set; }
    
    // Define the maximum number of clients allowed
    private const int MaxClients = 6;
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

        Timer timer = new();
        timer.Name = "PingTimer";
        timer.WaitTime = (double)Config.GetValue("game_constants", "ping_frequency_ms", true) / 1000;
        timer.Autostart = true;
        timer.Timeout += () => {
            foreach(long peerId in GameSessionManager.ConnectedPeers.Keys)
            {
                RpcId(peerId, MethodName.RequestPing, Time.GetUnixTimeFromSystem());
            }
        };
        AddChild(timer);
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Server.PeerDisconnected - Player disconnected: " + id);
        
        GameSessionManager.RemovePlayer(id);
    }

    private void PeerConnected(long id) {
        GD.Print("Server.PeerConnected - Player connected: " + id);

        if (GameSessionManager.GameHost == -1) { // First peer connected becomes host
            GameSessionManager.GameHost = id;
        }
        
        RpcId(id, MethodName.NotifyCurrentHost, GameSessionManager.GameHost);
        
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
            Rpc(MethodName.NotifyPlayerConnected, id, name, GameSessionManager.ConnectedPeers.Count);
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
        foreach(long peerId in GameSessionManager.ConnectedPeers.Keys)
        {
            RpcId(id, MethodName.NotifyPlayerConnected, peerId, GameSessionManager.ConnectedPeers[peerId].name, GameSessionManager.ConnectedPeers[peerId].sequenceNumber);
        }

        GameSessionManager.ConnectedPeers.Add(id, new PeerInfo{ name = name, sequenceNumber = sequenceNumber });
    }

    /// <summary>
    /// Server uses this to tell all clients to start their games
    /// </summary>
    /// <param name="filepath"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame(string filepath, int durationInSeconds, int maxAura) {
        if(Multiplayer.GetRemoteSenderId() == GameSessionManager.GameHost)
        {
            GD.Print("Server.NotifyStartGame - Telling clients to start their games");
            AcceptingConnections = false;
            Rpc(MethodName.NotifyStartGame, filepath, durationInSeconds, maxAura);

            PackedScene levelPackedScene = (PackedScene)ResourceLoader.Load(filepath);
            Node level = levelPackedScene.Instantiate();
            Root.Instance.AddChild(level); // using Root.Instance just to access tree
            GameSessionManager.GameDurationSeconds = durationInSeconds;
            GameSessionManager.MaxAura = maxAura;
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void RequestPing(double requestorTime)
    {
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.ReturnPing, requestorTime);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void ReturnPing(double requestorTime)
    {
        double ping = (Time.GetUnixTimeFromSystem() - requestorTime) / 2;
        double averagePing = GameSessionManager.ConnectedPeers[Multiplayer.GetRemoteSenderId()].AveragePing;
        List<double> lastPings = GameSessionManager.ConnectedPeers[Multiplayer.GetRemoteSenderId()].LastPings;
        lastPings.Add(ping);
        if(averagePing < 0) // if no ping is regestered then set it to the first ping
        {
            averagePing = ping;
        }
        else
        {
            if(lastPings.Count == 9)
            {
                double totalLatency = 0;
                lastPings.Sort();
                double midPoint = lastPings[4];
                for(int i = lastPings.Count - 1; i >= 0; i--)
                {
                    if(lastPings[i] > (2 * midPoint) && lastPings[i] > (20 / 1000)) // ignore outliers
                    {
                        lastPings.RemoveAt(i);
                    }
                    else
                    {
                        totalLatency += lastPings[i];
                    }
                }
                averagePing = totalLatency / lastPings.Count;
                lastPings.Clear();
            }
        }
        GameSessionManager.ConnectedPeers[Multiplayer.GetRemoteSenderId()].LastPings = lastPings;
        GameSessionManager.ConnectedPeers[Multiplayer.GetRemoteSenderId()].AveragePing = averagePing;
    }
}
