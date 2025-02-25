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
		[Export, ExportRequired]
		public Weapon WeaponType;
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
		[Export]
		public bool IsPassive = false; // Basically another CanFire used for sprinting or anytime you are "passive"
		[Export, ExportRequired, AnimationsRequired(new string[] { "Fire" })] 
		public AnimationPlayer APlayer { get; set; }
		[Export]
		public AudioStream ClickSound { get; set; }
		private Random rng = new();

		private int CurrentAmmo { get; set; }
		private int StoredAmmo { get; set; } // for charge upgrade
		private double TimeSinceLastCharge { get; set; } // for charge upgrade

        public override void _EnterTree()
        {
            SetMultiplayerAuthority(Name.ToString().Split('#').First().ToInt());
        }

        public override void _Ready()
		{
			GodotErrorService.ValidateRequiredData(this);
			LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].projectileSpawner.AddSpawnableScene(Projectile.ResourcePath);
			CurrentAmmo = MaxAmmo;

			foreach(VisualInstance3D mesh in GodotNodeFindingService.FindNodes<VisualInstance3D>(this))
			{
				if(IsMultiplayerAuthority()) // Move local meshes to overlay layer
				{
					mesh.SetLayerMaskValue(1, false);
					mesh.SetLayerMaskValue(2, true);
				}
			}
		}

        public override void _Process(double delta)
        {
			if(!IsMultiplayerAuthority()) return;
			
            TimeSinceLastCharge += delta;

			UI.InGameUI.AmmoCount.Show();
			UI.InGameUI.AmmoCount.SetAmmo(CurrentAmmo, MaxAmmo);
			
			if(GetCurrentFireMode() != FireModes.Single && Input.IsActionPressed("Fire"))
			{
				Fire();
			}
        }

		public override void _UnhandledInput(InputEvent @event)
    	{
			if(!IsMultiplayerAuthority()) return;

			if(Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				if(@event.IsActionPressed("Fire"))
				{
					TimeSinceLastCharge = 0; // Prevents storing a charge 

					// Click sound handled here so automatic guns don't spam the sound
                    if (CurrentAmmo <= 0 && MaxAmmo >= 0 && ClickSound != null)
                    {
                        PlayGunSound(ClickSound);
                    }
					else if (GetCurrentFireMode() == FireModes.Single)
					{
						Fire();
					}
				}
				if (@event.IsActionPressed("Reload"))
				{
					Reload();
				}
				if(@event.IsActionReleased("Fire"))
				{
					FireReleased();
				}
			}
		}

        public void Fire()
		{   
			if ((CurrentAmmo > 0 || MaxAmmo < 0) && CanFire && !IsPassive) 
			{	
				if(GetCurrentFireMode() != FireModes.Charge)
				{
					CurrentAmmo -= 1;
					APlayer.Play("Fire");
					
					FireProjectile();
				}
				else
				{
					if(TimeSinceLastCharge > APlayer.GetAnimation("Fire").Length) // we base how much time to charge based on firing time
					{
						CurrentAmmo -= 1;
						StoredAmmo++;
						TimeSinceLastCharge = 0;
					}
				}
			}
		}

		public void FireReleased()
		{
			if(GetCurrentFireMode() == FireModes.Charge) 
			{
				if(StoredAmmo > 0)
				{
					APlayer.Play("Fire");
					for(int i = 0; i < StoredAmmo; i++)
					{
						FireProjectile();
					}
					StoredAmmo = 0;
				}
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

		// Take in AudioSteam and then convert to string to support moving sound files
		public void PlayGunSound(AudioStream sound)
		{
			string soundPath = sound?.ResourcePath ?? string.Empty;
            MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, soundPath, this.GetPath(), Multiplayer.GetUniqueId(), "SFX");
		}

		private void FireProjectile()
		{
			List<int> angleOffsets = new(); // for each angleOffset a bullet will be fired with that offset
			if(LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode.Upgrades.Contains(Upgrade.W_DunceCap))
			{
				foreach(int angle in (int[]) Config.GetValue("Upgrade.W_DunceCap", "dunce_shot_offsets", true))
				{
					angleOffsets.Add(angle);
				}
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
					projectileInstance.GlobalTransform = LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode.ProjectileOrigin.GlobalTransform; // initial spot for local view
					LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].projectileParent.AddChild(projectileInstance, true);
				}
			}
		}

		private FireModes GetCurrentFireMode() 
		{
			FireModes fireMode = FireMode;
			// The reason why we handle the upgrade like this is to support multiple upgrades that might affect firemode. Aswell as the chance we have a weapon with default of charge
			if(LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode.Upgrades.Contains(Upgrade.W_MagDump))
			{
				fireMode = FireModes.Charge;
			}
			else if(LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode.Upgrades.Contains(Upgrade.W_SwitchFiremode))
			{
				if(FireMode == FireModes.Single) 
				{
					fireMode = FireModes.Automatic;
				}
				else if(FireMode == FireModes.Automatic)
				{
					fireMode = FireModes.Single;
				}
			}

			return fireMode;
		}
	}
}
