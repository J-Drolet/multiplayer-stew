using System;
using System.Collections.Generic;

public class RemoveUpgradeCommand : IConsoleCommand
{
    public string Command { get; set; } = "remove_upgrade";
    public string Description { get; set; } = "Removes upgrade from local player.";

    public ConsoleExecutionResult Execute(List<string> args)
    {
        ConsoleExecutionResult result = new ConsoleExecutionResult();
        if (args.Count > 1 && Enum.TryParse<Upgrade>(args[1], true, out Upgrade upgrade))
        {
            if (LevelManager.Instance == null || !LevelManager.Instance.LevelPeerInfo.ContainsKey(LevelManager.Instance.Multiplayer.GetUniqueId()))
            {
                result.message = "Cannot give upgrade, level isn't loaded";
                result.success = false;
            }
            else if(!LevelManager.Instance.LevelPeerInfo[LevelManager.Instance.Multiplayer.GetUniqueId()].characterNode.Upgrades.Contains(upgrade))
            {
                LevelManager.Instance.LevelPeerInfo[LevelManager.Instance.Multiplayer.GetUniqueId()].characterNode.RemoveUpgrade(upgrade);
                result.message = "Could not remove upgrade - player does not have the upgrade.";
                result.success = false;
            }
            else
            {
                LevelManager.Instance.LevelPeerInfo[LevelManager.Instance.Multiplayer.GetUniqueId()].characterNode.RemoveUpgrade(upgrade);
                result.message = "Removed upgrade " + upgrade.ToString();
                result.success = true;
            }
        }
        else
        {
            result.message = $"Usage: {Command} <upgrade_name>";
            result.success = false;
        }

        return result;
    }
}
