using Godot;
using multiplayerstew.Scripts.Attributes;
using System;

public partial class ScoreboardInfo : Control
{
    [Export, ExportRequired]
    public Label nameLabel;
    [Export, ExportRequired]
    public Label killsLabel;
    [Export, ExportRequired]
    public Label deathsLabel;
    [Export, ExportRequired]
    public Label maxPowerLevelLabel;
    [Export, ExportRequired]
    public Label auraLabel;
    [Export, ExportRequired]
    public Label pingLabel;
}
