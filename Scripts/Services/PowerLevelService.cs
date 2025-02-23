using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class PowerLevelService
{
    public static int GetPowerLevel(Upgrade upgrade)
    {
        return (int) Config.GetValue("Upgrade." + upgrade.ToString(), "power_level", true);
    }

    public static int GetPowerLevel(Weapon weapon)
    {
        return (int) Config.GetValue("Weapon." + weapon.ToString(), "power_level", true);
    }
}