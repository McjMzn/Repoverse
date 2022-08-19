using Repoverse.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repoverse
{
    public class Repoverse
    {
        public string WorkingDirectory { get; private set; }
        private int selectedNodeIndex = 0;
        private int activeShellIndex = 0;

        public Repoverse(string workingDirectory)
        {
            this.ChangeWorkingDirectory(workingDirectory);
        }

        public event EventHandler<IShell> ActiveShellChanged;
        
        public event EventHandler<ProcessResult> ProcessResultProvided;

        public WorkspaceNode Workspace { get; private set; }

        public List<IShell> Shells { get; private set; }

        public IShell ActiveShell => this.Shells[this.activeShellIndex];

        public WorkspaceNode SelectedNode => this.ActiveShell is ControlShell ? this.Workspace.RepositoryNodes[this.selectedNodeIndex] : null;

        public void ChangeWorkingDirectory(string workingDirectory)
        {
            this.WorkingDirectory = workingDirectory;
            this.Workspace = new WorkspaceNode(this.WorkingDirectory);
            this.Shells = new() { new RepoverseShell(this), new WorkingDirectoryShell(this), new ControlShell(this) };
        }

        public void ProcessKeyPress(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Tab)
            {
                this.ChangeShell();
                return;
            }

            this.ActiveShell.ProcessKeyPress(key);
        }

        public void ProcessControlKeyPress(ConsoleKeyInfo key)
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

                    this.InvokeProcessResultProvided(result);
                    break;
            }
        }
        
        public void InvokeProcessResultProvided(ProcessResult result)
        {
           this.ProcessResultProvided?.Invoke(this, result);
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
