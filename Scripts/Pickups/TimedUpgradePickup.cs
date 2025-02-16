using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;


namespace multiplayerstew.Scripts.Pickups
{
	public partial class TimedUpgradePickup : TimedPickup
	{

		[Export, ExportRequired]
		private Array<Upgrade> UpgradeSpawnList { get; set; } = new();
        [Export, ExportRequired]
        public Array<int> SpawnWeights = new();

        public Upgrade PickupUpgrade;

        public override void _Ready()
        {
            // if we forget some weights we add 1 to avoid bugs
            for(int i = SpawnWeights.Count; i < UpgradeSpawnList.Count; i++)
            {
                SpawnWeights.Add(1);
            }

            base._Ready();
        }

		protected override void ActivatePickup(Character character)
		{
            character.Rpc(Character.MethodName.AddUpgrade, (int)PickupUpgrade);
		}

        protected override Array<string> GetSpawnPaths()
        {   
            Array<string> spawnPaths = new();
            foreach(Upgrade upgrade in GetSpawnList())
            {
                spawnPaths.Add(EnumServices.GetFilePath(upgrade, Root.UpgradePickupFilepath));
            }

            return spawnPaths;
        }

        protected override PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {
            Array<Upgrade> spawnList = GetSpawnList();
            if(UpgradeSpawnList.Count == 0) // in the case we are in every Upgrade mode
            {
                PickupUpgrade = spawnList[rng.RandiRange(0, spawnList.Count - 1)];
            }
            else
            {   
                int totalWeight = 0;
                for(int i = 0; i < spawnList.Count; i++)
                {
                    totalWeight += SpawnWeights[i];
                }
                int selection = rng.RandiRange(0, totalWeight); // get a weight value for selection
                int culWeight = 0;
                // find what weapon the weight comes from
                for(int i = 0; i < spawnList.Count; i++)
                {
                    culWeight += SpawnWeights[i];
                    if(selection <= culWeight) 
                    {
                        PickupUpgrade = spawnList[i];
                        break;
                    }
                }
            }

            string sceneFilePath = EnumServices.GetFilePath(PickupUpgrade, Root.UpgradePickupFilepath);
            if(sceneFilePath == null)
            {
                return null;
            }
            else 
            {
                return ResourceLoader.Load<PackedScene>(sceneFilePath);
            }
        }

        private Array<Upgrade> GetSpawnList()
        {
            Array<Upgrade> spawnList;
            if(UpgradeSpawnList.Count == 0) 
            {
                spawnList = new((Upgrade[])Enum.GetValues(typeof(Upgrade)));
            }
            else
            {
                spawnList = UpgradeSpawnList;
            }

            return spawnList;
        }
    }
}
