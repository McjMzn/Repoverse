using Repoverse.Input;
using System;
using System.IO;

namespace Repoverse
{
    public class WorkingDirectoryShell : AnsiShell
    {
        private readonly Repoverse repoverse;

        public WorkingDirectoryShell(Repoverse repoverse)
        {
            this.repoverse = repoverse;
        }

        public override void ExecuteCommand(string command)
        {
            if (command.StartsWith("cd "))
            {
                var path = command.Substring(3);
                var combined = Path.IsPathRooted(path) ? path : Path.Combine(this.repoverse.WorkingDirectory, path);
                var fullCombined = Path.GetFullPath(combined);

                repoverse.ChangeWorkingDirectory(fullCombined);
                return;
            }
            else
            {
                ProcessRunner.Run(command, repoverse.WorkingDirectory);
            }

            lock (Locks.WorkspaceLock)
            {
                repoverse.Workspace.Update();
            }
        }

        public override string GetHelp()
        {
            return $"[grey]Command will be executed in working directory: [silver]{repoverse.WorkingDirectory}[/][/]";
        }

        public override string GetPrompt()
        {
            return $"{repoverse.WorkingDirectory}>";
        }
    }
}
