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

        public Upgrade PickupUpgrade;


		protected override void ActivatePickup(Character character)
		{
            character.Rpc(Character.MethodName.AddUpgrade, (int)PickupUpgrade);
		}

        protected override List<string> GetSpawnPaths()
        {   
            List<string> spawnPaths = new();
            foreach(Upgrade upgrade in GetSpawnList())
            {
                spawnPaths.Add(EnumServices.GetFilePath(upgrade, Root.UpgradePickupFilepath));
            }

            return spawnPaths;
        }

        protected override PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {
            Array<Upgrade> spawnList = GetSpawnList();

            PickupUpgrade = spawnList[rng.RandiRange(0, spawnList.Count - 1)];

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
