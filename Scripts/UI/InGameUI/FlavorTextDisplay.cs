using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class FlavorTextDisplay : Control
{
    [Export, ExportRequired]
    public Label TitleLabel;
    [Export, ExportRequired]
    public Label SubTitleLabel;
    [Export, ExportRequired]
    public Label PowerLevelLabel;
    [Export]
    public double TotalDisplayTime;
    [Export, ExportRequired]
    public Curve OpacityCurve;

    private double DisplayTime;

    public override void _Ready()
    {
        DisplayTime = 10000;
        Modulate = Colors.Transparent;
    }

    public override void _Process(double delta)
    {
        DisplayTime += delta;
        if(DisplayTime > TotalDisplayTime)
        {
            Modulate = Colors.Transparent;
        }
        else
        {
            float opacity = OpacityCurve.Sample((float)(DisplayTime / TotalDisplayTime));
            Modulate = Color.Color8(255, 255, 255, (byte)(255 * opacity));
        }
    }

    private void DisplayFlavorText(string title, string subtitle, int powerLevelChange)
    {
        DisplayTime = 0;
        TitleLabel.Text = title;
        SubTitleLabel.Text = subtitle;
        Modulate = Colors.White;

        if(powerLevelChange > 0)
        {
            PowerLevelLabel.Modulate = Colors.Green;
            PowerLevelLabel.Text = $"+{powerLevelChange}";
        }
        else if(powerLevelChange < 0)
        {
            PowerLevelLabel.Modulate = Colors.Red;
            PowerLevelLabel.Text = $"{powerLevelChange}";
        }
        else
        {
            PowerLevelLabel.Modulate = Colors.White;
            PowerLevelLabel.Text = $"+{powerLevelChange}";
        }

    }

    public void DisplayFlavorTextFor(Weapon weapon, Weapon? oldWeapon = null)
    {
        string title = (string)Config.GetValue("Weapon." + weapon.ToString(), "flavor_title", true);
        string subtitle = (string)Config.GetValue("Weapon." + weapon.ToString(), "flavor_subtitle", true);

        int weaponPowerLevel = PowerLevelService.GetPowerLevel(weapon);
        if(oldWeapon.HasValue)
        {
            weaponPowerLevel -= PowerLevelService.GetPowerLevel(oldWeapon.Value);
        }

        DisplayFlavorText(title, subtitle, weaponPowerLevel);
    }

    public void DisplayFlavorTextFor(Upgrade upgrade)
    {
        string title = (string)Config.GetValue("Upgrade." + upgrade.ToString(), "flavor_title", true);
        string subtitle = (string)Config.GetValue("Upgrade." + upgrade.ToString(), "flavor_subtitle", true);
        DisplayFlavorText(title, subtitle, PowerLevelService.GetPowerLevel(upgrade));
    }

    public void HideDisplay() 
    {
        DisplayTime = 10000;
    }
}
