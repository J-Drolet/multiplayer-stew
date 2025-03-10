using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;

public partial class InGameUI : Control
{
    [Export, ExportRequired]
    public AmmoDisplay AmmoCount { get; set; }
    [Export, ExportRequired]
    public Range HealthBar { get; set; }
    [Export, ExportRequired]
    public FlavorTextDisplay FlavorTextDisplay { get; set; }
    [Export, ExportRequired]
    public ItemDisplay ItemDisplay { get; set; }

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.InGameUI = this;
    }

    public void SetHealthBar(float healthPercent)
    {
        HealthBar.Value = healthPercent * 100;
    }
}
