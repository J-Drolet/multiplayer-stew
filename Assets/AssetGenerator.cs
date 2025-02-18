using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class AssetGenerator : Node3D
{
	[Export, ExportRequired]
	public string ASSET_FOLDER { get; set; } = "res://Assets/Models/TEST/";
    [Export, ExportRequired]
    public string EXPORT_FOLDER { get; set; } = "res://Assets/Models/EXPORT/";
    public override void _Ready()
	{
		GodotErrorService.ValidateRequiredData(this);
        AutomateSceneCreation();
	}

    private void AutomateSceneCreation()
    {
        using var dir = DirAccess.Open(ASSET_FOLDER);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (fileName.EndsWith(".glb"))
                {
                    CreateSceneFromGlb(fileName);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        else
        {
            GD.PrintErr($"Error: Could not open directory {ASSET_FOLDER}");
        }
    }

    private void CreateSceneFromGlb(string glbFileName)
    {
        string glbPath = Path.Combine(ASSET_FOLDER, glbFileName);
        PackedScene glbScene = GD.Load<PackedScene>(glbPath);

        if (glbScene != null)
        {
            Node sceneInstance = glbScene.Instantiate();
            // Extract All Mesh instances
            List<MeshInstance3D> meshs = sceneInstance.GetChildren().Where(c => c is MeshInstance3D).Select(c => c as MeshInstance3D).ToList();

            List<Node3D> newScenes = new();
            foreach (MeshInstance3D mesh in meshs)
            {
                Node3D generatedScene = GenerateScene(mesh);
                PackedScene newScene = new PackedScene();
                newScene.Pack(generatedScene);
                generatedScene.Free();
                string sceneName = glbFileName.Replace(".glb", ".tscn");
                string outputPath = Path.Combine(EXPORT_FOLDER, mesh.Name+sceneName);

                Error error = ResourceSaver.Save(newScene, outputPath);
                if (error == Error.Ok)
                {
                    GD.Print($"Saved scene: {outputPath}");
                }
                else
                {
                    GD.PrintErr($"Error saving scene: {outputPath}");
                }
            }
        }
        else
        {
            GD.PrintErr($"Error: Could not load GLB file {glbPath}");
        }
    }

    private Node3D GenerateScene(MeshInstance3D mesh)
    {
        Node3D root = new();
        AddChild(root);
        StaticBody3D staticBody = new();
        root.AddChild(staticBody);
        MeshInstance3D newMesh = mesh.Duplicate() as MeshInstance3D;
        newMesh.Position = Vector3.Zero;
        root.Rotation = newMesh.Rotation;
        root.Scale = newMesh.Scale;
        newMesh.Rotation = Vector3.Zero;
        newMesh.Scale = Vector3.One;
        staticBody.AddChild(newMesh);
        staticBody.SetOwner(root);
        newMesh.SetOwner(root);

        return root;
    }
}
