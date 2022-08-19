using Repoverse.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse
{
    public class WorkingDirectoryShell : AnsiShell
    {
        private readonly Repoverse repoverse;
        private SystemShell systemShell;
            
        public WorkingDirectoryShell(Repoverse repoverse)
        {
            this.repoverse = repoverse;
            this.systemShell = new SystemShell(repoverse.WorkingDirectory);
        }

        public event EventHandler<ProcessResult> ProcessResultProvided;

        public override void ExecuteCommand(string command)
        {
            var workdir = this.systemShell.WorkingDirectory;
            var output = this.systemShell.ExecuteCommand(command);
            var result = new ProcessResult
            {
                Command = command,
                ExitCode = this.systemShell.ExitCode,
                ProcessStandardOutput = output,
                WorkingDirectoryPath = workdir
            };

            this.repoverse.InvokeProcessResultProvided(result);

            if (workdir != this.systemShell.WorkingDirectory)
            {
                repoverse.ChangeWorkingDirectory(this.systemShell.WorkingDirectory);
            }

            lock (Locks.WorkspaceLock)
            {
                repoverse.Workspace.Update();
            }
        }

        public override string GetHelp()
        {
            return $"[grey]Command will be executed in the working directory.[/]";
        }

        public override string GetPrompt()
        {
            return $"{repoverse.WorkingDirectory}>";
        }
    }
}
