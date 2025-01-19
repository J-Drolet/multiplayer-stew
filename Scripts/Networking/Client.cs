using Godot;
using System;

public partial class Client : Node
{
    // Define the server's IP and port
    private string _serverIp = "127.0.0.1";  // Localhost for testing
    private const int ServerPort = 12345;
    private ENetMultiplayerPeer _client;

    // Called when the "Create Lobby" button is pressed
    public void CreateLobbyAndConnect()
    {
        StartServer();  // Start the server in headless mode
        ConnectToServer(_serverIp, ServerPort);  // Connect the client to the server
    }

    private void StartServer()
    {
        if (Engine.IsEditorHint())
        {
            string[] args = new string[] { "--headless", "server.tscn" };
            var exec = OS.Execute("godot", args, null, true);
            GD.Print("Started the server in headless mode.");
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

        if (err == Error.Ok)
        {
            GD.Print("Connected to the server.");
            Multiplayer.MultiplayerPeer = _client;
        }
        else
        {
            GD.PrintErr("Failed to connect: " + err);
        }
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
