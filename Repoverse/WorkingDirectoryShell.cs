using Repoverse.Input;
using Repoverse.OperatingSystem;
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
        private IOperatingSystemShell osShell;
            
        public WorkingDirectoryShell(Repoverse repoverse)
        {
            this.repoverse = repoverse;
            this.osShell = new WindowsCmd(repoverse.WorkingDirectory);
        }

        public event EventHandler<ProcessResult> ProcessResultProvided;

        public override void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            var startingWorkdir = this.osShell.ExecuteCommand("cd").Output;
            var shellResult = this.osShell.ExecuteCommand(command);
            var currentWorkdir = this.osShell.ExecuteCommand("cd").Output;

            var result = new ProcessResult
            {
                Command = command,
                ExitCode = shellResult.ExitCode,
                ProcessStandardOutput = shellResult.ExitCode == 0 ? shellResult.Output : string.Empty,
                ProcessStandardError = shellResult.ExitCode != 0 ? shellResult.Output : string.Empty,
                WorkingDirectoryPath = startingWorkdir
            };

            this.repoverse.InvokeProcessResultProvided(result);

            if (startingWorkdir != currentWorkdir)
            {
                repoverse.ChangeWorkingDirectory(currentWorkdir);
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
