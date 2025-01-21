using Godot;
using multiplayerstew.Scripts.Attributes;
using System;


namespace multiplayerstew.Scripts.Base
{
    public partial class Pickup: Node3D
    {
        [Export, ExportRequired]
        public Area3D PickupArea { get; set; }
        [Export, ExportRequired]
        public Mesh PickupMesh { get; set; }

    }
}
