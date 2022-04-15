using System;
using System.Collections.Generic;

namespace Repoverse.Input
{
    public class AnsiShell : IAnsiShell
    {
        private readonly AnsiShellInput input;
        private readonly Func<string> getPrompt;
        private readonly Func<string> getHelp;
        private readonly Action<string> executeCommand;

        public AnsiShell(Func<string> getPrompt, Func<string> getHelp, Action<string> executeCommand)
        {
            this.CommandHistory = new List<string>();
            this.input = new AnsiShellInput();
            this.getPrompt = getPrompt;
            this.getHelp = getHelp;
            this.executeCommand = executeCommand;
        }

        public List<string> CommandHistory { get; }
        public string RawInput => this.input.RawText;
        public string AnsiInput => this.input.AnsiText;
        public string Prompt => this.getPrompt();
        public string Help => this.getHelp();
        
        public void ProcessKeyPress(ConsoleKeyInfo key)
        {
            switch(key.Key)
            {
                case ConsoleKey.Enter:
                    this.ExecuteCommand();
                    break;

                default:
                    this.input.ProcessKeyPress(key);
                    break;
            }
        }
        
        public void ExecuteCommand()
        {
            this.executeCommand(this.input.RawText);
            this.CommandHistory.Add(this.input.RawText);
            this.input.Clear();
        }
    }
}
