using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Base;


namespace multiplayerstew.Scripts.Pickups
{
    public partial class WeaponPickup : Pickup
    {
        [Export, ExportRequired]
        private UpgradableWeapon Weapon { get; set; }
        protected override void ActivatePickup(Node3D body)
        {
            if (body.IsInGroup("Player") && body is Character)
            {
                Character character = body as Character;
                character.EquippedWeapon = Weapon;
            }
        }
    }
}
