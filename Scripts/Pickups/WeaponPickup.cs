using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;


namespace multiplayerstew.Scripts.Pickups
{
	public partial class WeaponPickup : Pickup
	{
		[Export, ExportRequired]
		private PackedScene Weapon { get; set; }

		public override void _Ready()
		{
			base._Ready();
			GodotErrorService.ValidateRequiredData(this);
		}

		protected override void ActivatePickup(Node3D body)
		{
			if (body is Character)
			{
				Character character = body as Character;
				character.EquippedWeapon = Weapon.Instantiate() as UpgradableWeapon;
				this.QueueFree();
			}
		}
	}
}
