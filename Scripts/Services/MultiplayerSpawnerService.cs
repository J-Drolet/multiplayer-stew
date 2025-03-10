using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multiplayerstew.Scripts.Services
{
    public class MultiplayerSpawnerService
    {
        public static void LoadMultiplayerSpawner(MultiplayerSpawner spawner, string filePath) 
        {
            List<string> files = GodotSceneFindingService.GetScenesAtFilepath(filePath);
            foreach (string file in files)
            {
                spawner.AddSpawnableScene(file);
            }
        }
    }
}
