using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse
{
    public class WorkspaceNode
    {
        private bool updateInProgress = false;
        
        public WorkspaceNode(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory does not exis: {path}");
            }

            this.Path = path;
            var repositoryPath = Repository.Discover(path);
            this.IsRepository = repositoryPath is not null;
            this.IsActive = this.IsRepository;
            this.Repository = repositoryPath is null ? null : new Repository(repositoryPath);
            if (this.IsRepository)
            {
                Task.Run(() => this.RepositoryStatus = this.Repository.RetrieveStatus(new StatusOptions()));
            }

            if (!this.IsRepository)
            {
                this.LoadSubdirectories(this);
            }
        }

        public string Path { get; }
        public bool IsRepository { get; private set; }
        public bool IsActive { get; set; }
        public bool IsSelected { get; set; }
        public Repository Repository { get; private set; }
        public RepositoryStatus RepositoryStatus { get; private set; }
        public List<ProcessResult> OperationResults { get; } = new List<ProcessResult>();
        public List<WorkspaceNode> Nodes { get; } = new List<WorkspaceNode>();
        public List<WorkspaceNode> RepositoryNodes => this.GetReposotoryNodes();
        public string Name => System.IO.Path.GetFileName(this.Path);
        public bool ContainRepositories => this.Nodes.Any(n => n.IsRepository || n.ContainRepositories);
        public bool HasActiveProcess { get; set; }
        public bool HasRecentResult { get; set; }

        public event Action<WorkspaceNode> Changed;

        private List<WorkspaceNode> GetReposotoryNodes()
        {
            if (this.IsRepository)
            {
                return new List<WorkspaceNode> { this };
            }

            if (this.Nodes is null || this.Nodes.Count == 0)
            {
                return new List<WorkspaceNode> { };
            }

            return this.Nodes.SelectMany(node => node.GetReposotoryNodes()).ToList();
        }

        private void LoadSubdirectories(WorkspaceNode parent)
        {
            var subdirectories = Directory.GetDirectories(parent.Path);
            if (subdirectories.Count() == 0)
            {
                return;
            }

            foreach (var subdirectory in subdirectories)
            {
                var node = new WorkspaceNode(subdirectory);
                parent.Nodes.Add(node);
            }
        }

        public void Update()
        {
            if (this.updateInProgress)
            {
                return;
            }
            
            this.updateInProgress = true;
            var repositoryPath = Repository.Discover(this.Path);
            if (repositoryPath is not null)
            {
                this.IsRepository = true;
                this.Repository = new Repository(repositoryPath);
                // this.RepositoryStatus = this.IsRepository ? this.Repository.RetrieveStatus(new StatusOptions()) : null;
                return;
            }

            // Remove nodes representing directories that don't exist anymore.
            this.Nodes
                .Where(node => !Directory.Exists(node.Path))
                .ToList()
                .ForEach(node => this.Nodes.Remove(node));

            var subdirectories = Directory.GetDirectories(this.Path);

            // Update active nodes.
            this.Nodes.ForEach(node => node.Update());

            // Add new nodes.
            var newNodes = subdirectories
                .Where(subdirectoryPath => this.Nodes.All(node => node.Path != subdirectoryPath))
                .ToList()
                .Select(subdirectoryPath => new WorkspaceNode(subdirectoryPath));

            this.Nodes.AddRange(newNodes);
            this.updateInProgress = false;
        }
    }
}
