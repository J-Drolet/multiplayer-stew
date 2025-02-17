using System;
using Godot;
using Godot.Collections;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;


namespace multiplayerstew.Scripts.Pickups
{
	public partial class TimedHealthPickup : TimedPickup
	{

		[Export, ExportRequired]
        public int HealthGained;

        protected override void ActivatePickup(Character character)
		{
			character.DamageTakenThisFrame -= HealthGained;
		}

        protected override Array<string> GetSpawnPaths()
        {   
            Array<string> spawnPaths = new();
            spawnPaths.Add(EnumServices.GetFilePath("Health", Root.HealthPickupFilepath));

            return spawnPaths;
        }

        protected override PackedScene GetRespawn() // marked virtual so I can put it in _Process
        {


            string sceneFilePath = EnumServices.GetFilePath("Health", Root.HealthPickupFilepath);
            if(sceneFilePath == null)
            {
                return null;
            }
            else 
            {
                return ResourceLoader.Load<PackedScene>(sceneFilePath);
            }
        }

        protected override bool CanPickup(Character character) // marked virtual so I can put it in _Process
        {
            return character.CurrentHealth < character.MaxHealth;
        }
    }
}
