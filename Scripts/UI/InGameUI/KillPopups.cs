using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;

public partial class KillPopups : VBoxContainer
{
    [Export, ExportRequired]
    public KillPopup KillPopupPrototype;
    [Export, ExportRequired]
    public double TotalDisplayTime;
    [Export, ExportRequired]
    public Curve OpacityCurve;

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.KillPopups = this;
        KillPopupPrototype.Hide(); // just in case I forgot to hide in editor
    }

    public void DisplayKill(string killedPlayerName, int auraGained)
    {
        KillPopup killPopup = (KillPopup)KillPopupPrototype.Duplicate();
        killPopup.KillLabel.Text = $"(+{auraGained}) Killed {killedPlayerName}";
        killPopup.TotalDisplayTime = TotalDisplayTime;
        killPopup.OpacityCurve = OpacityCurve;
        killPopup.Visible = true;
        KillPopupPrototype.AddSibling(killPopup); // moves to right after prototype i.e. on top of the list
    }
}
