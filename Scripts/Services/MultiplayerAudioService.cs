using Godot;
using System;
using System.Security.Cryptography.X509Certificates;


namespace multiplayerstew.Scripts.Services
{
    public partial class MultiplayerAudioService : Node
    {
        public static MultiplayerAudioService Instance { get; private set; }
        public override void _Ready()
        {
            Instance = this;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void PlaySound(string audioStreamPath, NodePath parentPath, bool is3D, string bus)
        {
            if (Multiplayer.IsServer()) return; // server doesn't play sounds
            Node parent = GetNode(parentPath);

            if (parent != null)
            {
                Node3D parent3D = parent as Node3D;

                AudioStream audioStream = ResourceLoader.Load(audioStreamPath) as AudioStream;

                if(audioStream != null)
                {
                    if (!is3D) // no locational audio for local player
                    {
                        AudioStreamPlayer audioStreamPlayer = new();
                        audioStreamPlayer.Stream = audioStream;
                        audioStreamPlayer.Bus = bus;
                        AddChild(audioStreamPlayer);
                        audioStreamPlayer.Play();
                        audioStreamPlayer.Finished += audioStreamPlayer.QueueFree;
                    }
                    else
                    {
                        AudioStreamPlayer3D audioStreamPlayer = new();
                        audioStreamPlayer.Stream = audioStream;
                        audioStreamPlayer.GlobalTransform = parent3D.GlobalTransform;
                        audioStreamPlayer.Bus = bus;
                        AddChild(audioStreamPlayer);
                        audioStreamPlayer.Play();
                        audioStreamPlayer.Finished += audioStreamPlayer.QueueFree;
                    }
                }
            }
        }
    }
}
