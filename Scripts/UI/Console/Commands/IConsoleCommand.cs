using System.Collections.Generic;

public interface IConsoleCommand
{
    public string Command { get; set; }
    public string Description { get; set; }
    
    public ConsoleExecutionResult Execute(List<string> args);
}