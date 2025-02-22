using System;
using Godot;
using Godot.Collections;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;


namespace multiplayerstew.Scripts.Pickups
{
	public partial class TimedWeaponPickup : TimedPickup
	{

		[Export, ExportRequired]
        private Array<Weapon> WeaponSpawnList { get; set; } = new();

        [Export, ExportRequired]
        public Array<int> SpawnWeights = new();

        public Weapon WeaponUpgrade;

        public override void _Ready()
        {
            // if we forget some weights we add 1 to avoid bugs
            for(int i = SpawnWeights.Count; i < WeaponSpawnList.Count; i++)
            {
                SpawnWeights.Add(1);
            }

            base._Ready();
        }

        protected override void ActivatePickup(Character character)
		{
			character.Rpc(Character.MethodName.SetWeapon, (int)WeaponUpgrade);
		}

        protected override Array<string> GetSpawnPaths()
        {   
            Array<string> spawnPaths = new();
            foreach(Weapon weapon in GetSpawnList())
            {
                spawnPaths.Add(EnumServices.GetFilePath(weapon, Root.WeaponPickupFilepath));
            }

            return spawnPaths;
        }

        protected override PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {
            Array<Weapon> spawnList = GetSpawnList();
            if(WeaponSpawnList.Count == 0) // in the case we are in every weapon mode
            {
                WeaponUpgrade = spawnList[rng.RandiRange(0, spawnList.Count - 1)];
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
                        WeaponUpgrade = spawnList[i];
                        break;
                    }
                }
            }

            string sceneFilePath = EnumServices.GetFilePath(WeaponUpgrade, Root.WeaponPickupFilepath);
            if(sceneFilePath == null)
            {
                return null;
            }
            else 
            {
                return ResourceLoader.Load<PackedScene>(sceneFilePath);
            }
        }

        private Array<Weapon> GetSpawnList()
        {
            Array<Weapon> spawnList;
            if(WeaponSpawnList.Count == 0) 
            {
                spawnList = new((Weapon[])Enum.GetValues(typeof(Weapon)));
            }
            else
            {
                spawnList = WeaponSpawnList;
            }

            return spawnList;
        }
    }
}
