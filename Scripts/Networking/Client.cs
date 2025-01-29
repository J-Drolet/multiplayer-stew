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
    private ENetMultiplayerPeer Peer;
    private int ServerPid = -1;

    public override void _ExitTree()
    {
        CloseServer();
        Config.WriteConfig();

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
                GD.Print("Client.StartServer - Started the server in headless mode.");
            }
            else {
                GD.Print("Client.StartServer - Failed to start server instance.");
            }
        }
        else
        {
            GD.Print("Client.StartServer - Running in a build");
        }
    }

    private void CloseServer()
    {
        if (ServerPid != -1) // kill headless server if this client created one
        {
            OS.Kill(ServerPid);
        }
    }

    /// <summary>
    /// Disconnects the client from the server and handles closing the server if client was the host
    /// </summary>
    public void Disconnect()
    {
        if (GameManager.GameHost == Multiplayer.GetUniqueId()) // if client is the host, close the server
        {
            CloseServer();
        }

        GameManager.Players.Clear();
        Peer.Close();
    }

    /// <summary>
    /// Tell the server to start the game. This will only hope if calling peer is the host
    /// </summary>
    public void StartGame(string levelPath) {
        GD.Print("starting " + levelPath);
        RpcId(1, MethodName.NotifyStartGame, levelPath);
    }

    public void ConnectToServer(string ip, int port)
    {
        // Set up the client network connection to the server
        Peer = new ENetMultiplayerPeer();
        Error err = Peer.CreateClient(ip, port);
        if(err != Error.Ok) {
            GD.PrintErr("Client.ConnectToServer - Error: " + err);
        }
    
        Multiplayer.MultiplayerPeer = Peer;
        Peer.GetConnectionStatus();
    }

    // called only on clients
    private void ConnectedToServer()
    {
        GD.Print("Connected to server");
        UI.ToggleSpinner(false);
        RpcId(1, MethodName.SendPlayerInfo, Multiplayer.GetUniqueId(), Config.GetValue("settings", "player_name").ToString());
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
        UI.Lobby.RefreshLobby();
        if (id == 1) // if the server disconnected we get kicked back to main menu
        {
            GameManager.LeaveJoinedGame();
            UI.ErrorMessage.DisplayError("Server disconnected");
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerInfo(long id, string name) {}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyPlayerConnected(long id, string name, int sequenceNumber) {
        GameManager.Players.Add(id, new GameManager.PlayerInfo{ name = name, sequenceNumber = sequenceNumber });
        UI.Lobby.RefreshLobby();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyStartGame(string filepath)
    {
        UI.MainMenu.CloseAllWindows();
        UI.MainMenu.Hide();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyCurrentHost(long hostId) {
        GameManager.GameHost = hostId;
        UI.Lobby.SetHost(hostId);
    }

    /// <summary>
    /// Server uses this to tell a client that the connection was refused and why
    /// </summary>
    /// <param name="reason"></param>
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void NotifyConnectionRefused(string reason) {
        GD.Print("Client.NotifyConnectionRefused - Connection refused: " + reason);
        UI.DisplayError(reason);
    }
}
