using System;
using System.Collections.Generic;
using System.Linq;

namespace Repoverse.Input
{
    public abstract class AnsiShell : SimpleShell, IAnsiShell
    {
        private readonly AnsiShellInput input;
        private int historyIndex = -1;

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
                case ConsoleKey.UpArrow:
                    this.historyIndex =
                            this.historyIndex == -1 ? this.CommandHistory.Count - 1 :
                            this.historyIndex > 0 ? this.historyIndex - 1 :
                            0;

                    this.input.RawText = this.CommandHistory[this.historyIndex];

                    break;

                case ConsoleKey.DownArrow:
                    if (this.historyIndex == this.CommandHistory.Count - 1)
                    {
                        this.historyIndex = -1;
                    }

                    if (this.historyIndex == -1)
                    {
                        this.CommandHistory.Last();
                        break;
                    }

                    this.historyIndex++;
                    this.input.RawText = this.CommandHistory[this.historyIndex];
                    break;

                case ConsoleKey.Enter:
                    this.ExecuteCommand(this.input.RawText);
                    this.CommandHistory.Add(this.input.RawText);
                    this.historyIndex = -1;
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
