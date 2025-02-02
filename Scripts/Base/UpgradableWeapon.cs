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

		[Export, ExportRequired]
		public AudioStream FiringSound { get; set; }
        [Export]
        public AudioStream UnloadSound { get; set; }
        [Export]
        public AudioStream LoadSound { get; set; }
		[Export]
		public AudioStream RackSound { get; set; }

        private int CurrentAmmo { get; set; }
		public List<Upgrade> Upgrades { get; set; } = new();


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
            foreach(Upgrade upgrade in Upgrades) 
            {
                upgrade.OnFire(this);
            }

			if ((CurrentAmmo > 0 || MaxAmmo < 0) && CanFire) 
			{
				APlayer.Play("Fire");
				CurrentAmmo -= 1;
				for (int x = ProjectilePerShot; x > 0; x--)
				{
					UpgradeableProjectile projectileInstance = Projectile.Instantiate() as UpgradeableProjectile;
					projectileInstance.Name = GetMultiplayerAuthority().ToString() +"#";
					projectileInstance.GlobalTransform = GameManager.Players[GetMultiplayerAuthority()].characterNode.ProjectileOrigin.GlobalTransform; // initial spot for local view
					GameManager.Players[GetMultiplayerAuthority()].projectileParent.AddChild(projectileInstance, true);
				}
			}
		}

		// Triggered by character
		public void Reload()
		{
			if (CurrentAmmo < MaxAmmo && APlayer.HasAnimation("Reload"))
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

		public void PlayGunSound(string sound)
		{
			switch (sound)
			{
				case "Fire":
                    MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, FiringSound.ResourcePath, this.GetPath(), !IsMultiplayerAuthority(), "SFX");
					break;
                case "Unload":
                    MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, UnloadSound.ResourcePath, this.GetPath(), !IsMultiplayerAuthority(), "SFX");
                    break;
                case "Load":
                    MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, LoadSound.ResourcePath, this.GetPath(), !IsMultiplayerAuthority(), "SFX");
                    break;
                case "Rack":
                    MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, RackSound.ResourcePath, this.GetPath(), !IsMultiplayerAuthority(), "SFX");
                    break;
                default:
					break;
            }
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
	}
}
