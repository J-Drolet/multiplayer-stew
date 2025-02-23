using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Character : Entity
{
	[Export, ExportRequired]
	public Camera3D Camera { get; set; }
	[Export, ExportRequired]
	public Node3D ProjectileOrigin { get; set; }
	[Export, ExportRequired]
	public Node3D Head { get; set; }
	[Export, ExportRequired]
	public Node3D Hand { get; set; }
	[Export, ExportRequired]
	public MeshInstance3D CharacterMesh { get; set; }
	[Export, ExportRequired, AnimationsRequired(new string[] {"Walk"})]
	public AnimationPlayer APlayer { get; set; }
    [Export, ExportRequired]
    public AnimationTree ATree { get; set; }
	[Export, ExportRequired]
	public GpuParticles3D InvisibilitySmokeParticles { get; set; }
	[Export, ExportRequired]
	public MultiplayerSpawner WeaponSpawner { get; set; }
	[Export, ExportRequired]
	public AudioStream OutlinePlayerSFX { get; set; }
	public HashSet<Upgrade> Upgrades { get; set; } = new();

	public bool CanMove { get; set; } = true; // whether or not the local player should be able to manipulate the character
	public bool CanLook { get; set; } = true; // whether or not the local player should be able to manipulate the character
	private int JumpsSinceHitGround { get; set; } // keeps track of how many jumps the character has done since last hitting the ground
	private double TimeSinceXray { get; set; } // for see through walls upgrade
	public double SlowdownMultiplier { get; set; } = 1.0;

    [Export]
    public UpgradableWeapon EquippedWeapon;

	public float BaseSpeed = (float)Config.GetValue("upgrade_constants", "base_speed", true);
	public float SprintMultiplier = (float)Config.GetValue("upgrade_constants", "sprint_multiplier", true);
	public float JumpVelocity = (float)Config.GetValue("upgrade_constants", "jump_velocity", true);

	public override void _EnterTree()
	{
		CurrentHealth = MaxHealth; // we have to do this here or else infinite spawns will happen on server side
		SetMultiplayerAuthority(Name.ToString().Split("#").First().ToInt());
		WeaponSpawner.GetParent().SetMultiplayerAuthority(1); // weapon spawning is server responsibility
		LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode = this;
		WeaponSpawner.Spawned += OnWeaponSpawned; // used to sync up EquippedWeapon
		WeaponSpawner.Despawned += OnWeaponDespawned;

		if(IsMultiplayerAuthority()) // local client requests its spawn point from server
		{
			LevelManager.Instance.RpcId(1, LevelManager.MethodName.RequestSpawnPoint);
			UI.InGameUI.AmmoCount.Visible = EquippedWeapon != null; // hide ammoCount on respawn
			UI.InGameUI.FlavorTextDisplay.HideDisplay();
			ATree.Active = true;

			UI.InGameUI.Show();
			Input.MouseMode = Input.MouseModeEnum.Captured;
        	UI.GunViewCamera.active = true;
		}
	}


    public override void _Ready()
    {
        base._Ready();

		List<string> files = GodotSceneFindingService.GetScenesAtFilepath(Root.WeaponsFilepath);
		foreach(string filepath in files) 
		{
			WeaponSpawner.AddSpawnableScene(filepath);
		}
    }

    public override void _UnhandledInput(InputEvent @event)
    {
		if(!IsMultiplayerAuthority()) return;

		if(CanLook)
		{
			if(@event is InputEventMouseMotion)
			{
				InputEventMouseMotion iEvent = @event as InputEventMouseMotion;
				float MouseSensitivity = (float) Config.GetValue("settings", "mouse_sensitivity");
				Head.RotateY(-iEvent.Relative.X * MouseSensitivity * 0.0001f);
                Camera.RotateX(-iEvent.Relative.Y * MouseSensitivity * 0.0001f);
				Camera.RotationDegrees = new Vector3(Math.Clamp(Camera.RotationDegrees.X, -85, 85), 0, 0);
				
            }
		}
			

        #region MovementAnimationHandling
        AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)ATree.Get("parameters/playback");
        bool isMoving = Input.GetVector("Left", "Right", "Forward", "Back") != Vector2.Zero;
		bool isPassive = false;
        if (isMoving && !Input.IsActionPressed("Sprint"))
		{
            stateMachine.Travel("Walk");

        }
		else if(isMoving && Input.IsActionPressed("Sprint"))
		{
			stateMachine.Travel("Sprint");
			isPassive = true;
		}
		else if(!isMoving)
		{
            stateMachine.Travel("Idle");
        }

		if (EquippedWeapon != null) 
		{ 
		EquippedWeapon.IsPassive = isPassive;
		}
        #endregion
    }

    public override void _Process(double delta)
    {
		base._Process(delta);

        #region UpgradeLogic
        ////////// Invisibility upgrade
        float transparency = Upgrades.Contains(Upgrade.C_Invisibility)? (float)Config.GetValue("upgrade_constants", "invisibility_transparency", true) : 0;
		CharacterMesh.Transparency = transparency;
		foreach(GeometryInstance3D mesh in GodotNodeFindingService.FindNodes<GeometryInstance3D>(Hand))
		{
			mesh.Transparency = transparency;
		}
		InvisibilitySmokeParticles.Emitting = Upgrades.Contains(Upgrade.C_Invisibility);

		if(!IsMultiplayerAuthority()) return;

		UI.InGameUI.AmmoCount.Visible = EquippedWeapon != null;

		////////// Small Player Upgrade
		Scale = Vector3.One * (!Upgrades.Contains(Upgrade.C_SmallerHitbox) ? 1.0f : (float)Config.GetValue("upgrade_constants", "small_hitbox_factor", true));

		////////// Outline Players Upgrade
		if(Upgrades.Contains(Upgrade.C_OutlinePlayers))
		{
			double beforeFrameTime = TimeSinceXray;
			TimeSinceXray += delta;
			double upgradeDuration = (double)Config.GetValue("upgrade_constants", "outline_players_duration", true);

			if(beforeFrameTime < upgradeDuration && TimeSinceXray >= upgradeDuration) // disable effect
			{
				foreach(int peerId in LevelManager.Instance.LevelPeerInfo.Keys)
				{
					if(peerId != Multiplayer.GetUniqueId()) // only add xray to peers
					{
						LevelManager.Instance.LevelPeerInfo[peerId].characterNode.CharacterMesh.Mesh.SurfaceSetMaterial(0, null);
					}
				}	
			}

			// enable effect
			if(TimeSinceXray >= (double)Config.GetValue("upgrade_constants", "outline_players_cooldown", true))
			{
				TimeSinceXray = 0;
				MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, OutlinePlayerSFX.ResourcePath, this.GetPath(), Multiplayer.GetUniqueId(), "SFX");

				foreach(long peerId in LevelManager.Instance.LevelPeerInfo.Keys)
				{
					if(peerId != Multiplayer.GetUniqueId()) // only add xray to peers
					{
						Material xrayMaterial = (Material) ResourceLoader.Load("res://Assets/Materials/Upgrades/XRay.tres");
						LevelManager.Instance.LevelPeerInfo[peerId].characterNode.CharacterMesh.Mesh.SurfaceSetMaterial(0, xrayMaterial);
					}
				}
			}
		}
		#endregion

		if (EquippedWeapon == null) UI.InGameUI.AmmoCount.Hide();

		UI.InGameUI.SetHealthBar(CurrentHealth / MaxHealth);

		SlowdownMultiplier = Mathf.Clamp(SlowdownMultiplier + ((double)Config.GetValue("upgrade_constants", "slowdown_recovery_strength", true) * delta), 0.0f, 1.0f);
    }

    public override void _PhysicsProcess(double delta)
	{
		if(!IsMultiplayerAuthority()) return;
		Vector3 velocity = Velocity;
		double acceleration = BaseSpeed;
		double deceleration = BaseSpeed;
		double speed = BaseSpeed;

        if (Input.IsActionPressed("Sprint"))
		{
			speed *= SprintMultiplier;
		}

		if(Upgrades.Contains(Upgrade.C_FastSlide))
		{
			speed *= 2;
			acceleration /= 10;
			deceleration /= 100;
		}

		int jumpsAllowedInAir = 0;
		if(Upgrades.Contains(Upgrade.C_DoubleJump))
		{
			jumpsAllowedInAir = 1;
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
			if(JumpsSinceHitGround == 0) // makes it so double jump doesn't allow 2 jumps in the air if fell of a ledge
			{
				JumpsSinceHitGround++;
			}
		}
		else 
		{
			JumpsSinceHitGround = 0; // reset jumps back to 0
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("Jump") && JumpsSinceHitGround <= jumpsAllowedInAir  && CanMove)
		{
			velocity.Y = JumpVelocity;
			JumpsSinceHitGround++;
		}

		Vector3 direction = Vector3.Zero;
		if(CanMove)
		{
			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
			direction = (Head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		}

		if (direction != Vector3.Zero)
		{
			Vector3 desiredVelocity;
			desiredVelocity.X = (float)(direction.X * speed);
			desiredVelocity.Z = (float)(direction.Z * speed);

			if(Math.Abs(desiredVelocity.X) > Math.Abs(velocity.X))
			{
				velocity.X = (float)Mathf.MoveToward(Velocity.X, desiredVelocity.X, acceleration);
			}
			else
			{
				velocity.X = (float)Mathf.MoveToward(Velocity.X, desiredVelocity.X, deceleration);
			}

			if(Math.Abs(desiredVelocity.Z) > Math.Abs(velocity.Z))
			{
				velocity.Z = (float)Mathf.MoveToward(Velocity.Z, desiredVelocity.Z, acceleration);
			}
			else
			{
				velocity.Z = (float)Mathf.MoveToward(Velocity.Z, desiredVelocity.Z, deceleration);
			}
		}
		else
		{
			velocity.X = (float)Mathf.MoveToward(Velocity.X, 0, deceleration);
			velocity.Z = (float)Mathf.MoveToward(Velocity.Z, 0, deceleration);
		}
		// apply slowdown - Only in X and Z - Gravity and jump is unaffected
		velocity.X *= (float)SlowdownMultiplier;
		velocity.Z *= (float)SlowdownMultiplier;

		Velocity = velocity; 
		MoveAndSlide();

    }

	public int CalculatePowerLevel()
	{
		int powerLevel = 0;

		foreach(Upgrade upgrade in Upgrades)
		{
			powerLevel += PowerLevelService.GetPowerLevel(upgrade);
		}

		if(EquippedWeapon != null) {
			powerLevel += PowerLevelService.GetPowerLevel(EquippedWeapon.WeaponType);
		}

		return powerLevel;
	}

	private void OnPowerLevelChange() 
	{
		if(Multiplayer.IsServer())
		{
			int characterOwner = GetMultiplayerAuthority();
			int powerLevel = CalculatePowerLevel();
			if(powerLevel > LevelManager.Instance.PlayerStats[characterOwner].maxPowerLevel)
			{
				LevelManager.Instance.PlayerStats[characterOwner].maxPowerLevel = powerLevel;
			}
		}
	}

    /// <summary>
    /// Moves the character to a spawn position. It is setup this way because local player has authority over its position, but server also needs to set an initial position
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetSpawnPoint(Vector3 spawnPosition, Vector3 spawnRotation)
	{
		if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast spawn positons

		GlobalPosition = spawnPosition;
		GlobalRotation = spawnRotation;

		Visible = true; // once spawned make visible
		Camera.MakeCurrent(); // dont want to see a split second before spawn is set
	}

	/// <summary>
	/// Only Called on Server. MultiplayerSpawner will sync the spawned weapon and peers will populate EquippedWeapon from the Event
	/// </summary>
	/// <param name="weaponUpgrade"></param>
	public void SetWeapon(Weapon weaponUpgrade)
	{
		if(!Multiplayer.IsServer()) return;

		if (EquippedWeapon != null)
		{
			EquippedWeapon.QueueFree();
			EquippedWeapon = null;
		}

		string scenePath = EnumServices.GetFilePath(weaponUpgrade, Root.WeaponsFilepath);

		if(scenePath != null)
		{
			PackedScene weaponScene = (PackedScene) ResourceLoader.Load(scenePath);
			UpgradableWeapon weapon = (UpgradableWeapon) weaponScene.Instantiate();
			Random nameGenerator = new(); // to get nonconflicting names. MultiplayerSpawner gets mixed up if not
            EquippedWeapon = weapon;
			EquippedWeapon.Name = GetMultiplayerAuthority().ToString() + "#" + nameGenerator.NextInt64(10000);
			
			WeaponSpawner.GetParent().AddChild(EquippedWeapon);
		}
		OnPowerLevelChange();
	}

	private void OnWeaponDespawned(Node node)
    {
        if(node == EquippedWeapon)
		{
			EquippedWeapon = null;
		}
    }

    private void OnWeaponSpawned(Node node)
    {
        if(node is UpgradableWeapon weapon)
		{
			EquippedWeapon = weapon;
			if(IsMultiplayerAuthority())
			{
				UI.InGameUI.FlavorTextDisplay.DisplayFlavorTextFor(weapon.WeaponType);
			}
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddUpgrade(Upgrade upgrade)
	{
		Upgrades.Add(upgrade);
		OnPowerLevelChange();
		if(IsMultiplayerAuthority())
		{
			UI.InGameUI.FlavorTextDisplay.DisplayFlavorTextFor(upgrade);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RemoveUpgrade(Upgrade upgrade)
	{
		Upgrades.Remove(upgrade);
	}

	/// <summary>
	/// For use for knockback
	/// </summary>
	/// <param name="knockbackAdd"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddKnockback(Vector3 knockbackAdd)
	{
		if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast spawn positons

		Velocity += knockbackAdd;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetSlowdown(float slowdownMultiplayer)
	{
		if(Multiplayer.GetRemoteSenderId() != 1) return; // only server should broadcast spawn positons

		SlowdownMultiplier = slowdownMultiplayer;
	}
}
