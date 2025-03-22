
using Godot;

/// <summary>
/// A grouping of SFX and tags.
/// </summary>
[Tool]
[GlobalClass]
public partial class FootProfileMaterialSpecification : Resource
{
    public enum MaterialType {
        GRASS,
        STONE,
        WOOD,
        SAND,
        GRAVEL,
        SNOW,
        METAL,
        WATER,
        FABRIC
    }

    /// <summary>
    /// The name for this specific Material; Used for labelling only.
    /// </summary>
    [Export]
    public MaterialType Material;


    /// <summary>
    /// Medium footsteps for walking
    /// </summary>
    [Export]
    public AudioStream[] WalkStepSFX;

    /// <summary>
    /// Hard footsteps for running
    /// </summary>
    [Export]
    public AudioStream[] RunStepSFX;

    /// <summary>
    /// Jumping off of the material SFX
    /// </summary>
    [Export]
    public AudioStream[] JumpSFX;

    /// <summary>
    /// Landing onto the material SFX
    /// </summary>
    [Export]
    public AudioStream[] LandingSFX;
}
