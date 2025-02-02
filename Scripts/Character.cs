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
	public List<CharacterUpgrade> Upgrades { get; set; } = new();

	public bool CanMove { get; set; } = true;
	
    [Export]
    private UpgradableWeapon equippedWeapon;
    public UpgradableWeapon EquippedWeapon
	{
		get { return equippedWeapon; }
		set {
            if (equippedWeapon != null)
            {
                equippedWeapon.QueueFree();
            }
            equippedWeapon = value;
			equippedWeapon.Name = GetMultiplayerAuthority().ToString() + ":EquippedWeapon";
			equippedWeapon.SetMultiplayerAuthority(GetMultiplayerAuthority());
            Hand.AddChild(equippedWeapon);

			// toggle ammo count display of local player
			if(IsMultiplayerAuthority())
			{
				UI.InGameUI.AmmoCount.Visible = equippedWeapon != null;
			}
        }	
	}

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public override void _EnterTree()
	{
		CurrentHealth = MaxHealth; // we have to do this here or else infinite spawns will happen on server side
		SetMultiplayerAuthority(Name.ToString().Split("#").First().ToInt());
		GameManager.Players[GetMultiplayerAuthority()].characterNode = this;
		
		if(IsMultiplayerAuthority()) // local client requests its spawn point from server
		{
			SceneManager.Instance.RpcId(1, SceneManager.MethodName.RequestSpawnPoint);
			UI.InGameUI.AmmoCount.Visible = equippedWeapon != null; // hide ammoCount on respawn
		}
	}

    public override void _UnhandledInput(InputEvent @event)
    {
		if(!IsMultiplayerAuthority()) return;

		if(Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (EquippedWeapon != null)
			{
				if ((EquippedWeapon?.FireMode == FireModes.Single) && @event.IsActionPressed("Fire"))
				{
					EquippedWeapon.Fire();
				}
				if (@event.IsActionPressed("Reload"))
				{
					EquippedWeapon.Reload();
				}
			}
			if(@event is InputEventMouseMotion)
			{
				InputEventMouseMotion iEvent = @event as InputEventMouseMotion;
				float MouseSensitivity = (float) Config.GetValue("settings", "mouse_sensitivity");
				Head.RotateY(-iEvent.Relative.X * MouseSensitivity * 0.0001f);
                Camera.RotateX(-iEvent.Relative.Y * MouseSensitivity * 0.0001f);
				Camera.RotationDegrees = new Vector3(Math.Clamp(Camera.RotationDegrees.X, -85, 85), 0, 0);
				
            }
		}
    }

    public override void _Process(double delta)
    {
		if (HealthText != null)
		{
			HealthText.Text = CurrentHealth <= 0.0f ? "Dead" : "Health: " + CurrentHealth.ToString();
		}

		if(!IsMultiplayerAuthority()) return;

		if (EquippedWeapon != null)
		{
			UI.InGameUI.AmmoCount.Text = EquippedWeapon.GetCurrentAmmoText();
			if (EquippedWeapon?.FireMode == FireModes.Automatic && Input.IsActionPressed("Fire"))
			{
				EquippedWeapon.Fire();
			}
        }

		UI.InGameUI.SetHealthBar(CurrentHealth / MaxHealth);
    }

    public override void _PhysicsProcess(double delta)
	{
		if(!IsMultiplayerAuthority()) return;
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("Jump") && IsOnFloor() && CanMove)
		{
			velocity.Y = JumpVelocity;
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
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
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
}
