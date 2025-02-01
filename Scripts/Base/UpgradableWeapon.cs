using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;


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

		private int CurrentAmmo { get; set; }
		public List<Upgrade> Upgrades { get; set; } = new();

		public override void _Ready()
		{
			GodotErrorService.ValidateRequiredData(this);
			GameManager.Players[GetMultiplayerAuthority()].projectileSpawner.AddSpawnableScene(Projectile.ResourcePath);
			CurrentAmmo = MaxAmmo;
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
				Rpc(MethodName.PlaySound, "fire");
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
			if (APlayer.HasAnimation("Reload"))
			{
				APlayer.Play("Reload");
			}
			else
			{
				GD.PushWarning($"No reload animation for {WeaponName}");
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

		/// <summary>
		/// Spawns an AudioStreamPlayer3D to play a
		/// </summary>
		/// <param name="soundType"></param>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		public void PlaySound(string soundType)
		{
			AudioStream audioStream;

			switch (soundType)
			{
				case "fire":
					audioStream = FiringSound;
					break;
				default:
					audioStream = null;
					break;
			}

			AudioStreamPlayer3D audioStreamPlayer = new();
			audioStreamPlayer.Stream = audioStream;
			audioStreamPlayer.GlobalTransform = GlobalTransform;
			audioStreamPlayer.Bus = "SFX";
			AddChild(audioStreamPlayer);
			audioStreamPlayer.Play();
			audioStreamPlayer.Finished += audioStreamPlayer.QueueFree;
		}
	}
}
