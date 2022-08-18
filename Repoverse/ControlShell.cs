using Repoverse.Input;
using System;

namespace Repoverse
{
    public class ControlShell : SimpleShell
    {
        private readonly Repoverse repoverse;

        public ControlShell(Repoverse repoverse)
        {
            this.repoverse = repoverse;
        }

        public override string GetHelp()
        {
            return "[silver]<Up>/<Down>[/] to navigate, [silver]<Space>[/] to toggle, [silver]<O>[/] to show output";
        }

        public override string GetPrompt()
        {
            return "";
        }

        public override void ProcessKeyPress(ConsoleKeyInfo key)
        {
            this.repoverse.ProcessControlKeyPress(key);
        }
    }
}
