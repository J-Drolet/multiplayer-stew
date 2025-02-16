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
	public partial class TimedWeaponPickup : TimedPickup
	{

		[Export, ExportRequired]
		private Array<Weapon> WeaponSpawnList { get; set; } = new();

        public Weapon WeaponUpgrade;


		protected override void ActivatePickup(Character character)
		{
			character.RpcId(character.GetMultiplayerAuthority(), Character.MethodName.SetWeapon, (int)WeaponUpgrade);
		}

        protected override List<string> GetSpawnPaths()
        {   
            List<string> spawnPaths = new();
            foreach(Weapon weapon in GetSpawnList())
            {
                spawnPaths.Add(EnumServices.GetFilePath(weapon, Root.WeaponPickupFilepath));
            }

            return spawnPaths;
        }

        protected override PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {
            Array<Weapon> spawnList = GetSpawnList();

            WeaponUpgrade = spawnList[rng.RandiRange(0, spawnList.Count - 1)];

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
