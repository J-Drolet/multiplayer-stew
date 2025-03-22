using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A collection of Material names, keywords, and SFX.
/// </summary>
[Tool]
[GlobalClass]
public partial class FootProfile : Resource
{
    FootProfileMaterialSpecification AirMaterialSpecification;

    [Export]
    public FootProfileMaterialSpecification[] Materials;

    public FootProfileMaterialSpecification StringToMaterial(string matString)
    {
        FootProfileMaterialSpecification.MaterialType material;
        if(string.IsNullOrEmpty(matString) || matString == "")
        {
            material = FootProfileMaterialSpecification.MaterialType.STONE; // Default to STONE if no material is specified
        }
        else
        {
            material = (FootProfileMaterialSpecification.MaterialType) Enum.Parse(typeof(FootProfileMaterialSpecification.MaterialType), matString, true);
        }

        foreach(FootProfileMaterialSpecification materialSpecification in Materials)
        {
            if(materialSpecification.Material == material)
            {
                return materialSpecification;
            }
        }
            
        return null;
    }
}