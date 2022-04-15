using Repoverse.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repoverse
{
    public class Repoverse
    {
        private readonly string workingDirectory;
        private int selectedNodeIndex = 0;
        private int activeShellIndex = 0;
        
        public Repoverse(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            this.Workspace = new WorkspaceNode(this.workingDirectory);
            this.Workspace.RepositoryNodes[this.selectedNodeIndex].IsSelected = true;

            var repoverseShell =
                new AnsiShell(
                    () => "Repoverse>",
                    () => "[grey]Command will be executed in every active repository.[/]",
                    command =>
                    {
                        ProcessRunner.Run(command, this.Workspace.RepositoryNodes, false);
                        lock(Locks.WorkspaceLock)
                        {
                            this.Workspace.Update();
                        }
                    }
                );

            var workingDirectoryShell =
                new AnsiShell
                (
                    () => $"{this.workingDirectory}>",
                    () => $"[grey]Command will be executed in working directory: [silver]{this.workingDirectory}[/][/]",
                    command => { }
                );
            
            var adminShell =
                new SimpleShell
                (
                    () => "",
                    () => "",
                    key => this.ProcessTreeKeyPress(key)
                );

            this.Shells = new() { repoverseShell, workingDirectoryShell, adminShell };
        }

        public event EventHandler<IShell> ActiveShellChanged;

        public event EventHandler<string> OutputMessageProduced;

        public WorkspaceNode Workspace { get; }

        public List<IShell> Shells { get; }

        public IShell ActiveShell => this.Shells[this.activeShellIndex];

        public void ProcessKeyPress(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Tab)
            {
                this.ChangeShell();
                return;
            }

            this.ActiveShell.ProcessKeyPress(key);
        }

        public void ProcessTreeKeyPress(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    this.ChangeSelectionIndex(1);
                    break;

                case ConsoleKey.UpArrow:
                    this.ChangeSelectionIndex(-1);
                    break;

                case ConsoleKey.Spacebar:
                    this.Workspace.RepositoryNodes[selectedNodeIndex].IsActive = !this.Workspace.RepositoryNodes[selectedNodeIndex].IsActive;
                    break;

                case ConsoleKey.O:
                    this.OutputMessageProduced?.Invoke(this, this.Workspace.RepositoryNodes[this.selectedNodeIndex].OperationResults.LastOrDefault()?.ProcessStandardOutput ?? string.Empty);
                    break;
            }
        }
        
        public void ChangeSelectionIndex(int offset)
        {
            this.Workspace.RepositoryNodes[selectedNodeIndex].IsSelected = false;
            this.selectedNodeIndex += offset;

            if (selectedNodeIndex < 0)
            {
                selectedNodeIndex = this.Workspace.RepositoryNodes.Count - 1;
            }
            
            if (selectedNodeIndex == this.Workspace.RepositoryNodes.Count)
            {
                selectedNodeIndex = 0;
            }
            
            this.Workspace.RepositoryNodes[selectedNodeIndex].IsSelected = true;
        }

        public void ChangeShell()
        {
            this.activeShellIndex = (this.activeShellIndex + 1) % this.Shells.Count;
            this.ActiveShellChanged?.Invoke(this, this.ActiveShell);
        }
    }
}
