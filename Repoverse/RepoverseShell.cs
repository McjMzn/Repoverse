using Repoverse.Input;

namespace Repoverse
{
    public class RepoverseShell : AnsiShell
    {
        private readonly Repoverse repoverse;

        public RepoverseShell(Repoverse repoverse)
        {
            this.repoverse = repoverse;
        }

        public override void ExecuteCommand(string command)
        {
            ProcessRunner.Run(command, repoverse.Workspace.RepositoryNodes, false);
            lock (Locks.WorkspaceLock)
            {
                repoverse.Workspace.Update();
            }
        }

        public override string GetHelp()
        {
            return "[grey]Command will be executed in every active repository.[/]";
        }

        public override string GetPrompt()
        {
            return "Repoverse>";
        }
    }
}
