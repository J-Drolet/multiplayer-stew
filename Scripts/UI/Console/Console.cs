using Godot;
using multiplayerstew.Scripts.Attributes;
using multiplayerstew.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;

public partial class Console : Control
{
    [Export, ExportRequired]
    public LineEdit CommandLine { get; set; }

    [Export, ExportRequired]
    public TextEdit Output { get; set; }

    private List<string> History = new List<string>(){"help"}; // default help command in history
    private int HistoryIndex = 0;

    private int NextUpIndex = 0;
    private int NextDownIndex = 0;

    public List<IConsoleCommand> Commands = new();

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);

        UI.Console = this;
        VisibilityChanged += OnVisibilityChanged;
        CommandLine.TextSubmitted += OnConsoleSubmit;
        CommandLine.KeepEditingOnTextSubmit = true;

        // dynamically load all console commands
        foreach(string filepath in GodotFileFindingService.GetFilesAtFilePath(Root.ConsoleCommandFilepath, ["cs"]))
        {
            string className = GodotFileFindingService.GetFileName(filepath);
            ObjectHandle objectHandle = Activator.CreateInstance(null, className);
            IConsoleCommand consoleCommand = (IConsoleCommand)objectHandle.Unwrap();
            Commands.Add(consoleCommand);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(CommandLine.IsEditing())
        {
            if(@event.IsActionPressed("ui_up"))
            {
                HistoryIndex = Math.Clamp(HistoryIndex + 1, 0, History.Count - 1);
                CommandLine.Text = History[HistoryIndex];

                GetViewport().SetInputAsHandled(); // make it so line edit doesn't do default behavior instead
            }
            else if(@event.IsActionPressed("ui_down"))
            {
                HistoryIndex = Math.Clamp(HistoryIndex - 1, -1, History.Count - 1);
                if(HistoryIndex < 0)
                {
                    CommandLine.Text = "";
                }
                else{
                    CommandLine.Text = History[HistoryIndex];
                }

                GetViewport().SetInputAsHandled(); // make it so line edit doesn't do default behavior instead
            }
        }
    }

    private void ResetHistoryIndex()
    {
        HistoryIndex = -1;
    }


    private void OnConsoleSubmit(string submittedText)
    {
        ConsoleExecutionResult result = ProcessCommand(submittedText.Split(" ").ToList());
        string returnText = result.message;
        Output.Text = returnText + "\n" + Output.Text;
        CommandLine.Text = "";

        ResetHistoryIndex();
    }

    public void OnVisibilityChanged()
    {
        if(Visible)
        {
            CommandLine.GrabFocus();
            CommandLine.Text = "";
            ResetHistoryIndex();
        }
        else
        {
            CommandLine.ReleaseFocus();
        }
    }

    private ConsoleExecutionResult ProcessCommand(List<string> args)
    {
        ConsoleExecutionResult result = new();;
        bool foundCommand = false;

        foreach(IConsoleCommand command in Commands)
        {
            if(args[0].ToLower() == command.Command.ToLower())
            {
                result = command.Execute(args);
                foundCommand = true;
                break;
            }
        }

        if(!foundCommand)
        {
            result.message = $"Unknown command: {args[0]}";
            result.success = false;
        }
        else 
        {
            string historyText = string.Join(" ", args);
            historyText = historyText.ToLower();
            if(History.Contains(historyText))
            {
                History.Remove(historyText); // we only add unique commands
            }
            History.Insert(0, historyText);
        }

        return result;
    }
}
