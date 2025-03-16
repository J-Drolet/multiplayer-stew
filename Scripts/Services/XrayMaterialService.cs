using System.Collections.Generic;
using Godot;

public static class XrayMaterialService 
{
    public static void MakeXrayEnabled(List<MeshInstance3D> meshInstances)
    {
        foreach(MeshInstance3D meshInstance in meshInstances)
        {
            MakeXrayEnabled(meshInstance);
        }
    }

    public static void MakeXrayEnabled(MeshInstance3D meshInstance)
    {
        BaseMaterial3D xrayMaterial = (BaseMaterial3D) ResourceLoader.Load("res://Assets/Materials/Upgrades/XRay.tres");
        for(int i = 0; i < meshInstance.GetSurfaceOverrideMaterialCount(); i++)
        {
            BaseMaterial3D xrayCopy = (BaseMaterial3D)xrayMaterial.Duplicate();
            BaseMaterial3D realMaterial = (BaseMaterial3D)meshInstance.Mesh.SurfaceGetMaterial(i).Duplicate();
            realMaterial.RenderPriority = 1; // needs to render after xray
            realMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha; 
            realMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always; // makes it so the depth test always happens, bug happened when xrayMaterial was transparent, engine thought you could see through it even when next pass was opaque
            xrayCopy.NextPass = realMaterial;
            meshInstance.SetSurfaceOverrideMaterial(i, xrayCopy);
        }
    }

    public static void ToggleXray(List<MeshInstance3D> meshInstances, bool enabled)
    {
        foreach(MeshInstance3D meshInstance in meshInstances)
        {
            ToggleXray(meshInstance, enabled);
        }
    }

    public static void ToggleXray(MeshInstance3D meshInstance, bool enabled)
    {
        for(int i = 0; i < meshInstance.GetSurfaceOverrideMaterialCount(); i++)
        {
            Material surfaceMaterial = meshInstance.GetSurfaceOverrideMaterial(i);
            if(surfaceMaterial != null && surfaceMaterial is BaseMaterial3D surfaceBaseMaterial)
            {
                surfaceBaseMaterial.NoDepthTest = enabled;
                Color xrayAlbedo = surfaceBaseMaterial.AlbedoColor;
                xrayAlbedo.A = enabled ? 1 : 0;
                surfaceBaseMaterial.AlbedoColor = xrayAlbedo;
            }
            else
            {
                GD.PrintErr("XrayMaterialService.ToggleXray - Error! Mesh " + meshInstance.Name + " is not setup as an Xray material");
            }
        }
    }

    public static void SetTransparencyOfVisibleMesh(List<MeshInstance3D> meshInstances, float alpha)
    {
        foreach(MeshInstance3D meshInstance in meshInstances)
        {
            SetTransparencyOfVisibleMesh(meshInstance, alpha);
        }
    }

    public static void SetTransparencyOfVisibleMesh(MeshInstance3D meshInstance, float alpha)
    {
        for(int i = 0; i < meshInstance.GetSurfaceOverrideMaterialCount(); i++)
        {
            Material surfaceMaterial = meshInstance.GetSurfaceOverrideMaterial(i);
            if(surfaceMaterial != null && surfaceMaterial is BaseMaterial3D surfaceBaseMaterial && surfaceBaseMaterial.NextPass != null && surfaceBaseMaterial.NextPass is BaseMaterial3D nextPassBaseMaterial)
            {
			    Color meshAlbedo = nextPassBaseMaterial.AlbedoColor;
			    meshAlbedo.A = alpha;
			    nextPassBaseMaterial.AlbedoColor = meshAlbedo;
            }
            else
            {
                GD.PrintErr("XrayMaterialService.SetTransparencyOfVisibleMesh - Error! Mesh " + meshInstance.Name + " is not setup as an Xray material with base material next pass");
            }
        }
    }
}