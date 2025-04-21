
using System.Collections.Generic;
using Godot;

public class ServerProjectilePathCommand : IConsoleCommand
{
    public string Command { get; set; } = "show_server_paths";
    public string Description { get; set; } = "Shows the path of the projectile based on authority position (real invisible bullet).";

    public ConsoleExecutionResult Execute(List<string> args)
    {
        ConsoleExecutionResult result = new ConsoleExecutionResult();
        if(args.Count > 1 && ConsoleUtils.GetBoolean(args[1]) != null)
        {
            Config.SetValue("debug", "view_server_path", (Variant)ConsoleUtils.GetBoolean(args[1]));
            result.message = Command + " " + args[1];
            result.success = true;
        }
        else
        {
            result.message = $"Usage: {Command} <true/false>";
            result.success = false;
        }

        return result;
    }
}