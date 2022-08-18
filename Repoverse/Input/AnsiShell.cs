using System;
using System.Collections.Generic;

namespace Repoverse.Input
{
    public abstract class AnsiShell : SimpleShell, IAnsiShell
    {
        private readonly AnsiShellInput input;

        public AnsiShell()
        {
            this.CommandHistory = new List<string>();
            this.input = new AnsiShellInput();
        }

        public List<string> CommandHistory { get; }
        public string RawInput => this.input.RawText;
        public string AnsiInput => this.input.AnsiText;
        
        public override void ProcessKeyPress(ConsoleKeyInfo key)
        {
            switch(key.Key)
            {
                case ConsoleKey.Enter:
                    this.ExecuteCommand(this.input.RawText);
                    this.CommandHistory.Add(this.input.RawText);
                    this.ClearInput();
                    break;

                default:
                    this.input.ProcessKeyPress(key);
                    break;
            }
        }
        
        public abstract void ExecuteCommand(string command);
    
        protected void ClearInput()
        {
            this.input.Clear();
        }
    }
}
