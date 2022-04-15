using System;

namespace Repoverse.Input
{
    public class SimpleShell : IShell
    {
        private readonly Func<string> getPrompt;
        private readonly Func<string> getHelp;
        private readonly Action<ConsoleKeyInfo> processKey;

        public SimpleShell(Func<string> getPrompt, Func<string> getHelp, Action<ConsoleKeyInfo> processKey)
        {
            this.getPrompt = getPrompt;
            this.getHelp = getHelp;
            this.processKey = processKey;
        }
        
        public string Prompt => this.getPrompt();
        public string Help => this.getHelp();

        public void ProcessKeyPress(ConsoleKeyInfo key)
        {
            this.processKey(key);
        }
    }
}
