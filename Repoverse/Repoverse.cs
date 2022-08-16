using Repoverse.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repoverse
{
    public class Repoverse
    {
        private IShell repoverseShell;
        private IShell workingDirectoryShell;
        private IShell controlShell;

        private readonly string workingDirectory;
        private int selectedNodeIndex = 0;
        private int activeShellIndex = 0;
        
        public Repoverse(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            this.Workspace = new WorkspaceNode(this.workingDirectory);

            this.repoverseShell =
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

            this.workingDirectoryShell =
                new AnsiShell
                (
                    () => $"{this.workingDirectory}>",
                    () => $"[grey]Command will be executed in working directory: [silver]{this.workingDirectory}[/][/]",
                    command => { }
                );
            
            this.controlShell =
                new SimpleShell
                (
                    () => "",
                    () => "[silver]<Up>/<Down>[/] to navigate, [silver]<Space>[/] to toggle, [silver]<O>[/] to show output",
                    key => this.ProcessTreeKeyPress(key)
                );

            this.Shells = new() { repoverseShell, workingDirectoryShell, controlShell };
        }

        public event EventHandler<IShell> ActiveShellChanged;
        
        public event EventHandler<ProcessResult> ProcessResultProvided;

        public WorkspaceNode Workspace { get; }

        public List<IShell> Shells { get; }

        public IShell ActiveShell => this.Shells[this.activeShellIndex];

        public WorkspaceNode SelectedNode => this.ActiveShell == this.controlShell ? this.Workspace.RepositoryNodes[this.selectedNodeIndex] : null;

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
                    var result = this.SelectedNode?.OperationResults?.LastOrDefault();
                    if (result is null)
                    {
                        return;
                    }

                    this.ProcessResultProvided?.Invoke(this, result);
                    break;
            }
        }
        
        public void ChangeSelectionIndex(int offset)
        {
            this.selectedNodeIndex += offset;

            if (selectedNodeIndex < 0)
            {
                selectedNodeIndex = this.Workspace.RepositoryNodes.Count - 1;
            }
            
            if (selectedNodeIndex == this.Workspace.RepositoryNodes.Count)
            {
                selectedNodeIndex = 0;
            }
        }

        public void ChangeShell()
        {
            this.activeShellIndex = (this.activeShellIndex + 1) % this.Shells.Count;
            this.ActiveShellChanged?.Invoke(this, this.ActiveShell);
        }
    }
}
