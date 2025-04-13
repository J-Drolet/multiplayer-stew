using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Character : Entity
{
	[Export, ExportRequired]
	public Camera3D Camera { get; set; }
	[Export, ExportRequired]
	public Node3D Head { get; set; }
	[Export, ExportRequired]
	public Node3D Hand { get; set; }
    public HashSet<Upgrade> Upgrades { get; set; } = new();
    [Export, ExportRequired]
    public Node3D PowerPackDisplayManager { get; set; }

    #region WeaponProperties
    [Export]
    public UpgradableWeapon EquippedWeapon;
    [Export, ExportRequired]
    public Node3D ProjectileOrigin { get; set; }
    [Export, ExportRequired]
    public MultiplayerSpawner WeaponSpawner { get; set; }
    #endregion

    #region AnimationProperties
    [Export, ExportRequired, AnimationsRequired(["Walk"])]
    public AnimationPlayer APlayer { get; set; }
    [Export, ExportRequired]
    public AnimationTree ATree { get; set; }
    #endregion

    #region CosmeticProperties
    [Export, ExportRequired]
    public MeshInstance3D CharacterMesh { get; set; }
	[Export, ExportRequired]
	public Node3D HeadCosmeticSlot { get; set; }
	[Export, ExportRequired]
	public Node3D FaceCosmeticSlot { get; set; }
    #endregion

    #region UpgradePropertiesAndFields
    [Export, ExportRequired]
    public GpuParticles3D InvisibilitySmokeParticles { get; set; }
    [Export, ExportRequired]
    public AudioStream OutlinePlayerSFX { get; set; }

	private Timer XrayTimer { get; set; }
	private Timer XrayDisableTimer { get; set; }
    public double SlowdownMultiplier { get; set; } = 1.0;
    private int JumpsSinceHitGround { get; set; } // keeps track of how many jumps the character has done since last hitting the ground
    #endregion

    public bool CanMove { get; set; } = true; // whether or not the local player should be able to manipulate the character
	public bool CanLook { get; set; } = true; // whether or not the local player should be able to manipulate the character
	public bool CanFire { get; set; } = true; // whether or not the local player should be able to shoot their gun

	// Signal on jump
    [Signal]
    public delegate void JumpEventHandler(bool isInAir);

	public override void _EnterTree()
	{
        CurrentHealth = MaxHealth; // we have to do this here or else infinite spawns will happen on server side
		SetMultiplayerAuthority(Name.ToString().Split("#").First().ToInt());
		WeaponSpawner.GetParent().SetMultiplayerAuthority(1); // weapon spawning is server responsibility
        LevelManager.Instance.LevelPeerInfo[GetMultiplayerAuthority()].characterNode = this;
		WeaponSpawner.Spawned += OnWeaponSpawned; // used to sync up EquippedWeapon
		WeaponSpawner.Despawned += OnWeaponDespawned;

        if (IsMultiplayerAuthority()) // local client requests its spawn point from server
		{
			LevelManager.Instance.RpcId(1, LevelManager.MethodName.RequestSpawnPoint);
			UI.InGameUI.AmmoCount.Visible = EquippedWeapon != null; // hide ammoCount on respawn
			UI.InGameUI.FlavorTextDisplay.HideDisplay();
			UI.InGameUI.ItemDisplay.Refresh(Upgrades);
			ATree.Active = true;

			UI.InGameUI.Show();
			Input.MouseMode = Input.MouseModeEnum.Captured;

            CharacterMesh.SetLayerMaskValue(1, false);
            CharacterMesh.SetLayerMaskValue(6, true);

			LevelManager.Instance.ToggleXrayEffect(false); // disable Xray effect on spawn
		}
		else
		{
			PowerPackDisplayManager.Hide();
		}

		// Load Cosmetics
		FaceCosmeticSlot.AddChild(ResourceLoader.Load<PackedScene>(GameSessionManager.ConnectedPeers[GetMultiplayerAuthority()].FaceCosmetic).Instantiate());
        HeadCosmeticSlot.AddChild(ResourceLoader.Load<PackedScene>(GameSessionManager.ConnectedPeers[GetMultiplayerAuthority()].HeadCosmetic).Instantiate());
		FaceCosmeticSlot.Visible = !IsMultiplayerAuthority();
		HeadCosmeticSlot.Visible = !IsMultiplayerAuthority();

		XrayMaterialService.MakeXrayEnabled(GetVisualMeshes());
    }

    public override void _Ready()
    {
        base._Ready();
        MultiplayerSpawnerService.LoadMultiplayerSpawner(WeaponSpawner, Root.WeaponsFilepath);
        SetWeapon(Weapon.Pistol);
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
		ToggleInvisibilityUpgrade(Upgrades.Contains(Upgrade.C_Invisibility));

		if(!IsMultiplayerAuthority()) return;

		UI.InGameUI.AmmoCount.Visible = EquippedWeapon != null;

		UI.GunViewCamera.GlobalTransform = Camera.GlobalTransform;
		UI.GunViewCamera.Fov = Camera.Fov;

		////////// Small Player Upgrade
		Scale = Vector3.One * (!Upgrades.Contains(Upgrade.C_SmallerHitbox) ? 1.0f : (float)Config.GetValue("Upgrade.C_SmallerHitbox", "small_hitbox_factor", true));

		////////// Outline Players Upgrade
		if(Upgrades.Contains(Upgrade.C_OutlinePlayers))
		{
			if(XrayTimer == null)
			{
				if(XrayDisableTimer == null)
				{
					XrayDisableTimer = new();
					XrayDisableTimer.Name = "XrayDisableTimer";
					XrayDisableTimer.WaitTime = (double)Config.GetValue("Upgrade.C_OutlinePlayers", "outline_players_duration", true);
					XrayDisableTimer.Autostart = false;
					XrayDisableTimer.OneShot = true;
					XrayDisableTimer.Timeout += () => {LevelManager.Instance.ToggleXrayEffect(false);};
					AddChild(XrayDisableTimer);
				}
				XrayTimer = new();
				XrayTimer.Name = "XrayEnableTimer";
				XrayTimer.WaitTime = (double)Config.GetValue("Upgrade.C_OutlinePlayers", "outline_players_cooldown", true);
				XrayTimer.Autostart = true;
				XrayTimer.Timeout += () => {
					LevelManager.Instance.ToggleXrayEffect(true);
					MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, OutlinePlayerSFX.ResourcePath, this.GetPath(), Multiplayer.GetUniqueId(), "SFX");
					XrayDisableTimer.Start();
				};
				AddChild(XrayTimer);
			}
		}
		#endregion

		if (EquippedWeapon == null) UI.InGameUI.AmmoCount.Hide();

		UI.InGameUI.SetHealthBar(CurrentHealth / MaxHealth);

		SlowdownMultiplier = Mathf.Clamp(SlowdownMultiplier + ((double)Config.GetValue("Upgrade.W_SlowTargetBullets", "slowdown_recovery_strength", true) * delta), 0.0f, 1.0f);
    }

    public override void _PhysicsProcess(double delta)
	{
		if(!IsMultiplayerAuthority()) return;
		Vector3 velocity = Velocity;
		double baseSpeed = (float)Config.GetValue("game_constants", "base_speed", true);

		double acceleration = baseSpeed;
		double deceleration = baseSpeed;
		double speed = baseSpeed;

        if (Input.IsActionPressed("Sprint"))
		{
			speed *= (float)Config.GetValue("game_constants", "sprint_multiplier", true);;
		}

		if(Upgrades.Contains(Upgrade.C_FastSlide))
		{
			speed *= (float)Config.GetValue("Upgrade.C_FastSlide", "speed_multiplier", true);
			acceleration *= (float)Config.GetValue("Upgrade.C_FastSlide", "acceleration_multiplier", true);;
			deceleration *= (float)Config.GetValue("Upgrade.C_FastSlide", "deceleration_multiplier", true);;
		}

		int jumpsAllowedInAir = 0;
		if(Upgrades.Contains(Upgrade.C_DoubleJump))
		{
			jumpsAllowedInAir = (int)Config.GetValue("Upgrade.C_DoubleJump", "jumps_in_air", true);
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			if(velocity.Y > 0)
			{
				velocity += GetGravity() * (float)delta;
			}
			else // on descent games generally increase gravity to reduce floatiness
			{
				float descentGravityScale = (float)Config.GetValue("game_constants", "gravity_scale_descent", true);
				velocity += GetGravity() * descentGravityScale * (float)delta;
			}
			
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
			EmitSignal("Jump", JumpsSinceHitGround == 0);
			velocity.Y = (float)Config.GetValue("game_constants", "jump_velocity", true);
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

	private void UpdatePowerLevelStats() 
	{
        int currentPowerLevel = CalculatePowerLevel();
        if (Multiplayer.IsServer())
		{
            int characterOwner = GetMultiplayerAuthority();
			if(currentPowerLevel > LevelManager.Instance.PlayerStats[characterOwner].maxPowerLevel)
			{
				LevelManager.Instance.PlayerStats[characterOwner].maxPowerLevel = currentPowerLevel;
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
		UpdatePowerLevelStats();
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
			Weapon? oldWeapon = null;
			if(EquippedWeapon != null) 
			{
				oldWeapon = EquippedWeapon.WeaponType;
			}

			EquippedWeapon = weapon;
			if(IsMultiplayerAuthority())
			{
				UI.InGameUI.FlavorTextDisplay.DisplayFlavorTextFor(weapon.WeaponType, oldWeapon);
			}
		}
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddUpgrade(Upgrade upgrade)
	{
		Upgrades.Add(upgrade);
		UpdatePowerLevelStats();
		if(IsMultiplayerAuthority())
		{
			UI.InGameUI.ItemDisplay.Refresh(Upgrades);
			UI.InGameUI.FlavorTextDisplay.DisplayFlavorTextFor(upgrade);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RemoveUpgrade(Upgrade upgrade)
	{
        Upgrades.Remove(upgrade);
		if (IsMultiplayerAuthority())
		{
			UI.InGameUI.ItemDisplay.Refresh(Upgrades);

			if(upgrade == Upgrade.C_OutlinePlayers)
			{
				LevelManager.Instance.ToggleXrayEffect(false); // disable xray in case we remove this
				
				if(XrayTimer != null)
				{
					XrayTimer.QueueFree();
					XrayTimer = null;
				}
				if(XrayDisableTimer != null)
				{
					XrayTimer.QueueFree();
					XrayTimer = null;
				}
			}
		}
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

	private void ToggleInvisibilityUpgrade(bool invisible)
	{
		float transparency = invisible ? (float)Config.GetValue("Upgrade.C_Invisibility", "invisibility_transparency", true) : 0;
		float alpha = 1 - transparency;
		XrayMaterialService.SetTransparencyOfVisibleMesh(GetVisualMeshes(), alpha);

		InvisibilitySmokeParticles.Emitting = invisible;
	}

	public List<MeshInstance3D> GetVisualMeshes()
	{
		List<MeshInstance3D> visualMeshes = new();
		foreach(MeshInstance3D faceCosmetic in GodotNodeFindingService.FindNodes<MeshInstance3D>(FaceCosmeticSlot))
		{
			visualMeshes.Add(faceCosmetic);
		}
		foreach(MeshInstance3D headCosmetic in GodotNodeFindingService.FindNodes<MeshInstance3D>(HeadCosmeticSlot))
		{
			visualMeshes.Add(headCosmetic);
		}
		visualMeshes.Add(CharacterMesh);

		return visualMeshes;
	}
}
