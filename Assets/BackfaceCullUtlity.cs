using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class BackfaceCullUtlity : Node3D
{
    [Export, ExportRequired]
	public string ASSET_FOLDER { get; set; } = "res://Assets/Models/TEST/";

    public override void _Ready()
	{
		GodotErrorService.ValidateRequiredData(this);
        AddBackfaceCulling();
        GetTree().Quit();
	}

    public void AddBackfaceCulling()
    {
        foreach(string filepath in GodotFileFindingService.GetScenesAtFilepath(ASSET_FOLDER))
        {
            GD.Print("Attempting to access: " + filepath);
            PackedScene scene = (PackedScene)ResourceLoader.Load(filepath);
            Node sceneNode = scene.Instantiate();
            AddChild(sceneNode);
            foreach(MeshInstance3D meshInstance in GodotNodeFindingService.FindNodes<MeshInstance3D>(sceneNode))
            {
                for(int i = 0; i < meshInstance.GetSurfaceOverrideMaterialCount(); i++)
                {
                    BaseMaterial3D mat = (BaseMaterial3D)meshInstance.Mesh.SurfaceGetMaterial(i);
                    mat.CullMode = BaseMaterial3D.CullModeEnum.Back;
                    meshInstance.Mesh.SurfaceSetMaterial(i, mat);
                }
            }
            
            scene.Pack(sceneNode);
            sceneNode.Free();

            ResourceSaver.Save(scene, filepath);

            GD.Print("Added backface culling to: " + filepath);
        }
    }
}
