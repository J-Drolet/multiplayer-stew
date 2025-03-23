using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FootStepEmitter : RayCast3D
{

    
    public static string AIR_NAME = "Air";

    [Export, ExportRequired]
    public Character Character;
    [Export, ExportRequired]
    public FootProfile FootProfile;


    [Export]
    public float FootstepDistance = 1.6f;
    public float WalkSpeed = 1.6f;
    public float RunSpeed = 3.2f;

    private bool WasLastOnFloor;
    private Vector3 LastPosition;
    private FootProfileMaterialSpecification LastFootProfileMaterial;
    private float CurDistanceTravelled;
    private RandomNumberGenerator RNG = new();
    private static Vector3 XZVector = new Vector3(1, 0, 1); // used to ignore Y axis when calculating distance travelled


    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        WasLastOnFloor = Character.IsOnFloor();
        LastPosition = Character.GlobalPosition;

        WalkSpeed = Character.BaseSpeed;
        RunSpeed = Character.SprintMultiplier * WalkSpeed;
        Character.Jump += OnJump;
    }

    private void OnJump(bool isInAir)
    {
        if(LastFootProfileMaterial != null && LastFootProfileMaterial.JumpSFX.Length > 0)
        {
            AudioStream audioStream = LastFootProfileMaterial.JumpSFX[RNG.RandiRange(0, LastFootProfileMaterial.JumpSFX.Length - 1)];
            MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, audioStream.ResourcePath, GetPath(), Multiplayer.GetUniqueId(), "SFX");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        bool IsOnFloor = Character.IsOnFloor();
        FootProfileMaterialSpecification footMaterial = GetFootMaterial();

        if(IsOnFloor)
        {
            if(!WasLastOnFloor) // play landing sound
            {
                if(footMaterial != null && footMaterial.LandingSFX.Length > 0)
                {
                    AudioStream audioStream = footMaterial.LandingSFX[RNG.RandiRange(0, footMaterial.LandingSFX.Length - 1)];
                    MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, audioStream.ResourcePath, GetPath(), Multiplayer.GetUniqueId(), "SFX");
                }
                CurDistanceTravelled = 0; // reset distance travelled on landing
                LastPosition = GlobalPosition * XZVector;
            }

            CurDistanceTravelled += LastPosition.DistanceTo(GlobalPosition * XZVector);
            if(CurDistanceTravelled > FootstepDistance)
            {
                CurDistanceTravelled = 0;

                if(footMaterial == null)
                {
                    return; // no material, no sound
                }

                float currentSpeed = Character.GetRealVelocity().Length();
                AudioStream[] stepsSfxArray;
                if(currentSpeed >= RunSpeed)
                {
                    stepsSfxArray = footMaterial.RunStepSFX;
                }
                else
                {
                    stepsSfxArray = footMaterial.WalkStepSFX;

                }

                AudioStream audioStream = stepsSfxArray[RNG.RandiRange(0, stepsSfxArray.Length - 1)];
                MultiplayerAudioService.Instance.Rpc(MultiplayerAudioService.MethodName.PlaySound, audioStream.ResourcePath, GetPath(), Multiplayer.GetUniqueId(), "SFX");
            }

        }

        WasLastOnFloor = IsOnFloor;
        LastPosition = Character.GlobalPosition * XZVector;
        LastFootProfileMaterial = footMaterial;
    }

    private FootProfileMaterialSpecification GetFootMaterial()
    {
        if(IsColliding())
        {
            GodotObject collider = GetCollider();
            string colliderTag = (string)collider.GetMeta("material", "");

            return FootProfile.StringToMaterial(colliderTag);
        }

        return null;
    }
}