using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;


namespace multiplayerstew.Scripts.Base
{
	public enum FireModes
	{
		// Some weapons with "None" will call "Fire()" in their animantion Ex. Melee weapons
		None,
		Single,
		Automatic,
		Charge
	}
	public partial class UpgradableWeapon : Node3D
	{
		// Define new weapon parameters + projectile as Godot Properties
		[Export]
		public string WeaponName { get; set; } = "Unknown";
		[Export]
		public int MaxAmmo { get; set; } = -1;
		[Export]
		public FireModes FireMode { get; set; } = FireModes.Single;
		[Export]
		public int ProjectilePerShot { get; set; } = 1;
		[Export, ExportRequired]
		public PackedScene Projectile { get; set; }
		[Export]
		public bool CanFire = true; // Trigger this in the Animation to set the rate of fire
		[Export, ExportRequired, AnimationsRequired(new string[] { "Fire" })] 
		public AnimationPlayer APlayer { get; set; }
		[Export]
		public string ClickSoundResourcePath { get; set; }

		private int CurrentAmmo { get; set; }
		public HashSet<WeaponUpgrade> Upgrades { get; set; } = new();

        public override void _EnterTree()
        {
            SetMultiplayerAuthority(Name.ToString().Split('#').First().ToInt());
        }

        public override void _Ready()
		{
			GodotErrorService.ValidateRequiredData(this);
			GameManager.Players[GetMultiplayerAuthority()].projectileSpawner.AddSpawnableScene(Projectile.ResourcePath);
			CurrentAmmo = MaxAmmo;

			// Move local meshes to overlay layer
			if(IsMultiplayerAuthority())
			{
				foreach(VisualInstance3D mesh in FindMeshes(this, new()))
				{
					mesh.SetLayerMaskValue(1, false);
					mesh.SetLayerMaskValue(2, true);
				}
			}

		}

		public void Fire()
		{   
			if ((CurrentAmmo > 0 || MaxAmmo < 0) && CanFire) 
			{	
				Random rng = new();
				APlayer.Play("Fire");
				CurrentAmmo -= 1;
				
				List<int> angleOffsets = new(); // for each angleOffset a bullet will be fired with that offset
				if(Upgrades.Contains(WeaponUpgrade.DunceCap))
				{
					angleOffsets.Add(-30);
					angleOffsets.Add(30);
				}
				else
				{
					angleOffsets.Add(0);
				}

				for (int x = ProjectilePerShot; x > 0; x--)
				{
					foreach(int angleOffset in angleOffsets)
					{
						UpgradeableProjectile projectileInstance = Projectile.Instantiate() as UpgradeableProjectile;
						projectileInstance.Name = GetMultiplayerAuthority().ToString() + "#" + rng.NextInt64(10000)  + "#" + angleOffset; // encode the owner of the projectile and the RNG seed
						projectileInstance.GlobalTransform = GameManager.Players[GetMultiplayerAuthority()].characterNode.ProjectileOrigin.GlobalTransform; // initial spot for local view
						GameManager.Players[GetMultiplayerAuthority()].projectileParent.AddChild(projectileInstance, true);
					}
                }
			}
			else if(CurrentAmmo <= 0 && MaxAmmo >= 0 && ClickSoundResourcePath != null)
			{
				PlayGunSound(ClickSoundResourcePath);
			}
		}

		// Triggered by character
		public void Reload()
		{
			if (CurrentAmmo < MaxAmmo && APlayer.HasAnimation("Reload") && !APlayer.IsPlaying())
			{
				APlayer.Play("Reload");
			}
		}

		// Called by Reload Animation or other
		public void RefillAmmo()
		{
			CurrentAmmo = MaxAmmo;
		}

		public string GetCurrentAmmoText()
		{
			return $"{CurrentAmmo}/{MaxAmmo}";
		}

		public void PlayGunSound(string soundPath)
		{
            MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, soundPath, this.GetPath(), Multiplayer.GetUniqueId(), "SFX");
		}

		/// <summary>
		/// Recursively loops through every node under parent and adds any VisualInstance3D nodes to the list. Useful for changing the visibility layer
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="meshes"></param>
		/// <returns></returns>
		private List<VisualInstance3D> FindMeshes(Node parent, List<VisualInstance3D> meshes)
		{
			foreach(Node child in parent.GetChildren(true))
			{
				if (child is VisualInstance3D)
				{
					meshes = meshes.Append((VisualInstance3D)child).ToList();
				}

				meshes = FindMeshes(child, meshes);
			}

			return meshes;
		} 

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		public void AddUpgrade(WeaponUpgrade upgrade)
		{
			Upgrades.Add(upgrade);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		public void RemoveUpgrade(WeaponUpgrade upgrade)
		{
			Upgrades.Remove(upgrade);
		}
	}
}
