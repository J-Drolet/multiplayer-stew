using Godot;
using System;
using System.Threading;

public partial class Client : Node
{
   
    private readonly string localHost = "127.0.0.1";
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
        /*Multiplayer.PeerConnected.connect(player_connected);
        Multiplayer.PeerDisconnected.connect(player_disconnected);
        Multiplayer.ConnectedToServer.connect(connected_to_server);
        Multiplayer.ConnectionFailed.connect(connection_failed);
        */

        Multiplayer.Connect("ConnectedToServer", new Callable(this, "ConnectedToServer"));
        Multiplayer.Connect("ConnectionFailed", new Callable(this, "ConnectionFailed"));

        CreateLobbyAndConnect();
    }

    // Called when the "Create Lobby" button is pressed
    public void CreateLobbyAndConnect()
    {
        StartServer();
        ConnectToServer(localHost, ServerPort);  // Connect the client to the server
    }

    private void StartServer()
    {
        if (OS.HasFeature("editor")) // different execution chains for when it is packaged and when it is being tested in editor
        { 
            string[] args = new string[] {  "Scenes/server.tscn" };//{ "--headless", "Scenes/server.tscn" };
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

    private void ConnectToServer(string ip, int port)
    {
        // Set up the client network connection to the server
        _client = new ENetMultiplayerPeer();
        Error err = _client.CreateClient(ip, port);
    
        Multiplayer.MultiplayerPeer = _client;
    }

    // called only on clients
    private void ConnectedToServer()
    {
        GD.Print("Connected to server");
        //main.connection_succeeded()
        //send_player_info.rpc_id(1, peer_name, peer_color, multiplayer.get_unique_id()
    }

    // called only on clients
    private void ConnectionFailed()
    {
        GD.Print("Failed Connection");
    }


    /*
	send_player_info(player_name, player_color, multiplayer.get_unique_id())
    ## called on join, allows keeping track of connected players
    @rpc("any_peer", "reliable")
    [Rpc(
    func send_player_info(player_name: String, player_color: Color, id: int) -> void:
        if accepting_connections == false: return # if we aren't accepting connections then we don't add new players
        
        if !game_manager.players.has(id):
            game_manager.players[id] = {
                "name": player_name,
                "id": id,
                "color": player_color
            }
        if multiplayer.is_server():
            for i: int in game_manager.players:
                rpc("send_player_info", game_manager.players[i].name, game_manager.players[i].color, i)
        
        main.update_player_list()
        */
}
