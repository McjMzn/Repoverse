using System;

namespace Repoverse.Input
{
    public abstract class SimpleShell : IShell
    {
        public SimpleShell()
        {
        }
        
        public string Prompt => this.GetPrompt();
        public string Help => this.GetHelp();

        public abstract string GetPrompt();
        public abstract string GetHelp();
        public abstract void ProcessKeyPress(ConsoleKeyInfo key);
    }
}
