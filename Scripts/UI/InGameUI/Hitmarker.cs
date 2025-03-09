

using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;

public partial class Hitmarker: Control
{
    [Export, ExportRequired]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export, ExportRequired]
    public TextureRect HitmarkerTexture { get; set; }
    [Export, ExportRequired]
    public AudioStream HitmarkerSound { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.Hitmarker = this;
    }

    public void DisplayHitmarker(bool isVital)
    {
        if(isVital)
        {
            HitmarkerTexture.SelfModulate = Colors.Red;
        }
        else
        {
            HitmarkerTexture.SelfModulate = Colors.YellowGreen;
        }

        AnimationPlayer.Stop();
        AnimationPlayer.Play("Hitmarker");
        MultiplayerAudioService.Instance.PlaySound(HitmarkerSound.ResourcePath, this.GetPath(), false, "Hitmarker");
    }
}