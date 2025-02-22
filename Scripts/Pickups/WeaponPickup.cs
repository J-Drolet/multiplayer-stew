using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;
using multiplayerstew.Scripts.Services;


namespace multiplayerstew.Scripts.Pickups
{
	public partial class WeaponPickup : Pickup
	{
		[Export, ExportRequired]
		private Weapon weapon { get; set; }


		protected override void ActivatePickup(Character character)
		{
			character.Rpc(Character.MethodName.SetWeapon, (int)weapon);
		}
	}
}
