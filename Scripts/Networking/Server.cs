using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class ServerData
{
    public int ServerID { get; set; }
    public string ServerName { get; set; }
    public int Port { get; set; }
    public int Players { get; set; }
    public bool InLobby { get; set; }
}

public partial class Server : Node
{
    
    public static Server Instance { get; private set; }
    public ServerData ServerData { get; set; } = new ServerData() 
    { 
        Port = 3333
    };
    // If server is managed by server manager. Will output data file in server data folder
    public bool IsManaged { get; set; } = false;
    public string ServerDataFilepath { get; set; }
    private string ServerDataFile { get { return Path.Combine(ServerDataFilepath, $"SD_{ServerData.ServerID}.json"); }}
    
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
        Error err = Peer.CreateServer(ServerData.Port, MaxClients);

        if (err != Error.Ok)
        {
            GD.PrintErr("Server._Ready - Error starting the server: " + err);
        }
        else
        {
            GD.Print($"Server._Ready - Server started successfully on port {ServerData.Port}.");
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
        OnServerDataChanged();
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Server.PeerDisconnected - Player disconnected: " + id);
        
        GameSessionManager.RemovePlayer(id);

        // Close server if no players are left
        if (GameSessionManager.ConnectedPeers.Count == 0)
        {
            GD.Print("Server.PeerDisconnected - No players left, closing server");
            Multiplayer.MultiplayerPeer.Close();
            GetTree().Quit(); // Quit the game
        }
        OnServerDataChanged();
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

        OnServerDataChanged();
    }

    /// <summary>
    /// Client uses this to tell the server about themselves
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerInfo(long id, string peerInfoJson) {
        if(AcceptingConnections)
        {
            PeerInfo peerInfo = JsonSerializer.Deserialize<PeerInfo>(peerInfoJson);
            peerInfo.sequenceNumber = GameSessionManager.ConnectedPeers.Count;
            peerInfoJson = JsonSerializer.Serialize(peerInfo);
            Rpc(MethodName.NotifyPlayerConnected, id, peerInfoJson);
        }
    }

    /// <summary>
    /// Server uses this to tell all clients about a new player that connected. Tells the client who connected about all previous connections
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerConnected(long id, string peerInfoJson) {
        // For the peer that connected, we have to tell them about all other players that joined before them
        foreach(long peerId in GameSessionManager.ConnectedPeers.Keys)
        {
            RpcId(id, MethodName.NotifyPlayerConnected, peerId, JsonSerializer.Serialize(GameSessionManager.ConnectedPeers[peerId]));
        }

        GameSessionManager.ConnectedPeers.Add(id, JsonSerializer.Deserialize<PeerInfo>(peerInfoJson));
    }

    /// <summary>
    /// Server uses this to tell all clients to start their games
    /// </summary>
    /// <param name="filepath"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame(string levelPath, string gameInfoJson) {
        if(Multiplayer.GetRemoteSenderId() == GameSessionManager.GameHost)
        {
            GameSessionManager.GameInfo = JsonSerializer.Deserialize<GameInfo>(gameInfoJson);

            GD.Print("Server.NotifyStartGame - Telling clients to start their games");
            AcceptingConnections = false;
            Rpc(MethodName.NotifyStartGame, gameInfoJson);

            PackedScene levelPackedScene = (PackedScene)ResourceLoader.Load(levelPath);
            Node level = levelPackedScene.Instantiate();
            Root.Instance.AddChild(level); // using Root.Instance just to access tree
        }
        else if (Multiplayer.IsServer() && IsManaged)
        {
            OnServerDataChanged();
            ServerData.InLobby = false;
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

        // Using LevelManager.Instance.PlayerStats allows automatic syncing of the ping values to all clients
        if(LevelManager.Instance?.PlayerStats.GetValueOrDefault(Multiplayer.GetRemoteSenderId()) != null)
        {
            LevelManager.Instance.PlayerStats[Multiplayer.GetRemoteSenderId()].ping = (int)(ping * 1000);
        }
    }

    private void OnServerDataChanged()
    {
        if(IsManaged && !string.IsNullOrEmpty(ServerDataFilepath))
        {
            try
            {
                ServerData.Players = GameSessionManager.ConnectedPeers.Count;
                //write file to disk 
                string json = JsonSerializer.Serialize(ServerData, new JsonSerializerOptions { WriteIndented = true });
                
                File.WriteAllText(@ServerDataFile, json);
            }
            catch (Exception e)
            {
                GD.PrintErr("Server.OnServerDataChanged - Error writing server data to file: " + e.Message);
            }
        }
    }
}
