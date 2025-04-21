
using System.Collections.Generic;
using Godot;

public class HelpCommand : IConsoleCommand
{
    public string Command { get; set; } = "help";
    public string Description { get; set; } = "Gets a list of all available commands.";

    public ConsoleExecutionResult Execute(List<string> args)
    {
        ConsoleExecutionResult result = new ConsoleExecutionResult();

        foreach(IConsoleCommand command in UI.Console.Commands)
        {
            result.message += $"{command.Command}: {command.Description}\n";
        }

        result.success = true;

        return result;
    }
}