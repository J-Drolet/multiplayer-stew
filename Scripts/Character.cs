using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;
using System;

public partial class Character : CharacterBody3D
{
	[Export, ExportRequired]
	public Camera3D Camera { get; set; }
	[Export, ExportRequired]
	public Node3D ProjectileOrigin { get; set; }
	[Export, ExportRequired]
	public Node3D Head { get; set; }
	[Export, ExportRequired]
	public Node3D Hand { get; set; }
	[Export]
	private UpgradableWeapon equippedWeapon;
	[Export, ExportRequired]
	private Label AmmoLabel { get; set; }
    public UpgradableWeapon EquippedWeapon
	{
		get { return equippedWeapon; }
		set { EquipWeapon(value); }	
	}

    [Export]
	public int MouseSensitivity { get; set; } = 50;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(Name.ToString().ToInt());
	}

    public override void _Ready()
    {
		GodotErrorService.ValidateRequiredData(this);
	
		if(!IsMultiplayerAuthority()) return;

		GD.Print($"Peer {Multiplayer.GetUniqueId()} is making character {Name} the current");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Camera.MakeCurrent();
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
			
			if (@event.IsActionPressed("ui_cancel"))
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }

			if(@event is InputEventMouseMotion)
			{
				InputEventMouseMotion iEvent = @event as InputEventMouseMotion;
				Head.RotateY(-iEvent.Relative.X * MouseSensitivity * 0.0001f);
                Camera.RotateX(-iEvent.Relative.Y * MouseSensitivity * 0.0001f);
				Camera.RotationDegrees = new Vector3(Math.Clamp(Camera.RotationDegrees.X, -85, 85), 0, 0);
				
            }
		}
			
    }

    public override void _Process(double delta)
    {
		if(!IsMultiplayerAuthority()) return;

		if (EquippedWeapon != null)
		{
			AmmoLabel.Text = EquippedWeapon.GetCurrentAmmoText();
			if (EquippedWeapon?.FireMode == FireModes.Automatic && Input.IsActionPressed("Fire"))
			{
				EquippedWeapon.Fire();
			}
		}
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
		if (Input.IsActionJustPressed("Jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
		Vector3 direction = (Head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
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

	public void EquipWeapon(UpgradableWeapon weapon)
	{
		if(!IsMultiplayerAuthority()) return;

		if (EquippedWeapon != null)
		{
			EquippedWeapon.QueueFree();
		}
		equippedWeapon = weapon;
        EquippedWeapon.ProjectileOrigin = ProjectileOrigin; 
        Hand.AddChild(EquippedWeapon);
    }
}
