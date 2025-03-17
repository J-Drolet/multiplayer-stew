using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Diagnostics;

public partial class MeshManipulationUtlity : Node3D
{
    public enum UtilityMode{
        BackfaceCulling,
        LightmapGeneration
    }

    [Export, ExportRequired]
	public string ASSET_FOLDER { get; set; } = "res://Assets/Models/TEST/";
    [Export, ExportRequired]
    public UtilityMode UtilMode = UtilityMode.BackfaceCulling;

    public override void _Ready()
	{
		GodotErrorService.ValidateRequiredData(this);
        switch(UtilMode)
        {
            case UtilityMode.BackfaceCulling:
                AddBackfaceCulling();
                break;
            case UtilityMode.LightmapGeneration:
                GenerateLightmaps();
                break;
        }
        GetTree().Quit();
	}

    public void AddBackfaceCulling()
    {
        foreach(string filepath in GodotFileFindingService.GetScenesAtFilepath(ASSET_FOLDER))
        {
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

    public void GenerateLightmaps()
    {
        foreach(string filepath in GodotFileFindingService.GetScenesAtFilepath(ASSET_FOLDER))
        {
            PackedScene scene = (PackedScene)ResourceLoader.Load(filepath);
            Node sceneNode = scene.Instantiate();
            AddChild(sceneNode);
            foreach(MeshInstance3D meshInstance in GodotNodeFindingService.FindNodes<MeshInstance3D>(sceneNode))
            {
                if(meshInstance.Mesh is ArrayMesh arrayMesh)
                {
                    ArrayMesh shadowMesh = arrayMesh.ShadowMesh;
                    shadowMesh = null;
                    shadowMesh.Free();
                    arrayMesh.LightmapUnwrap(meshInstance.Transform, 0.1f);
                }
                else
                {
                    GD.PrintErr("Failed to add lightmap to " + filepath + " - not an arraymesh");
                }
            }
            
            scene.Pack(sceneNode);
            sceneNode.Free();

            ResourceSaver.Save(scene, filepath);

            GD.Print("Added Lightmap to: " + filepath);
        }
    }
}
