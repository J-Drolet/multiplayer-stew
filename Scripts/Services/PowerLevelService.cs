using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class PowerLevelService
{
    public static int GetPowerLevel(Upgrade upgrade)
    {
        return (int) Config.GetValue("power_levels", "Upgrade." + upgrade.ToString());
    }

    public static int GetPowerLevel(Weapon weapon)
    {
        return (int) Config.GetValue("power_levels", "Weapon." + weapon.ToString());
    }
}