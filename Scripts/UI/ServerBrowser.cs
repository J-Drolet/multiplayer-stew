using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;

public partial class ServerBrowser : Control
{

    private static readonly string ServerFavoritesFilepath = "user://ServerList.json";

    [Export, ExportRequired]
    public ServerUiElement ServerPrototype { get; set; }
    [Export, ExportRequired]
    Control LobbyPage { get; set; }
    
    // For adding Server
    [Export, ExportRequired]
    public LineEdit ServerNameInput;
    [Export, ExportRequired]
    public LineEdit IpAddressInput;
    [Export, ExportRequired]
    public LineEdit PortInput;

    public Dictionary<string, Dictionary<int, string>> Favorites { get; set; }

    public Dictionary<string, Dictionary<int, ServerUiElement>> ServerUiElements { get; set; } = new();

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        UI.ServerBrowser = this;

        Favorites = JsonFileService.ReadFromDisk<Dictionary<string, Dictionary<int, string>>>(ServerFavoritesFilepath);
        if(Favorites == null) /// in case the File doesn't exist
        {
            Favorites = new();
        }

        foreach(string ipAddress in Favorites.Keys)
        {
            foreach(int port in Favorites[ipAddress].Keys)
            {
                CreateServerElement(ipAddress, port, Favorites[ipAddress][port]);
            }
        }
    }


    public void ConnectToServer(string ip, int port) 
    {
        Client.Instance.ConnectToServer(ip, port);
        UI.Spinner.Show();
    }

    public void AddFavorite(string ip, int port, string serverName)
    {
        if(Favorites.ContainsKey(ip) && Favorites[ip].ContainsKey(port))
        {
            string oldServerName = Favorites[ip][port];
            UI.ErrorMessage.DisplayError($"Cannot create server. Server with name {oldServerName} exists and already points to the same IP and port");
        }
        else
        {
            if(!Favorites.ContainsKey(ip))
            {
                Favorites.Add(ip, new());
            }
            Favorites[ip].Add(port, serverName);
            JsonFileService.WriteToDisk(ServerFavoritesFilepath, Favorites);
            CreateServerElement(ip, port, serverName);
        }
    }

    public void DeleteFavorite(string ip, int port)
    {
        Favorites[ip].Remove(port);
        if(Favorites[ip].Count == 0)
        {
            Favorites.Remove(ip);
        }

        ServerUiElements[ip][port].QueueFree();
        ServerUiElements[ip].Remove(port);
        if(ServerUiElements[ip].Count == 0)
        {
            ServerUiElements.Remove(ip);
        }
        
        JsonFileService.WriteToDisk(ServerFavoritesFilepath, Favorites);
    }

    private void CreateServerElement(string ip, int port, string serverName)
    {
        ServerUiElement serverUiElement = (ServerUiElement)ServerPrototype.Duplicate();
        ServerPrototype.GetParent().AddChild(serverUiElement);
        serverUiElement.ServerName.Text = serverName;
        serverUiElement.IpAddress.Text = ip;
        serverUiElement.PortNumber.Text = port.ToString();
        serverUiElement.JoinButton.Pressed += () => ConnectToServer(ip, port);
        serverUiElement.DeleteButton.Pressed += () => DeleteFavorite(ip, port);
        serverUiElement.Show();

        if(!ServerUiElements.ContainsKey(ip)) {
            ServerUiElements.Add(ip, new());
        }
        ServerUiElements[ip].Add(port, serverUiElement);
    }

    public void OnAddServerButtonPressed()
    {
        AddFavorite(IpAddressInput.Text, PortInput.Text.ToInt(), ServerNameInput.Text);
    }
}
