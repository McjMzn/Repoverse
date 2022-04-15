using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse.Rendering
{
    public class WorkspaceRenderer : IRenderable
    {
        private Tree tree;
        public WorkspaceNode Workspace { get; set; }

        public WorkspaceRenderer(WorkspaceNode node)
        {
            this.Workspace = node;
        }

        private Tree CreateTree(WorkspaceNode workspace)
        {
            lock (Locks.WorkspaceLock)
            {
                var tree = new Tree(new Markup($"[yellow]{workspace.Path}[/]"));
                AddTreeNodes(workspace, tree);
                return tree;
            }
        }

        public void AddTreeNodes(WorkspaceNode workspace, IHasTreeNodes parentNode)
        {
            foreach (var workspaceNode in workspace.Nodes)
            {
                if (!workspaceNode.ContainRepositories && !workspaceNode.IsRepository)
                {
                    continue;
                }

                var color = workspaceNode.IsRepository ? "blue" : "default";
                var branchIndicator = workspaceNode.IsRepository ? $"[cyan] ({workspaceNode.Repository.Head.FriendlyName})[/]" : string.Empty;
                var isSelectedIndicator = workspaceNode.IsSelected ? $"> " : string.Empty;
                var isActiveIndicator =
                    !workspaceNode.IsRepository ? string.Empty :
                    workspaceNode.IsActive ? $"{isSelectedIndicator}[white]■ [/]" : $"{isSelectedIndicator}[grey]■ [/]";

                var operationResultIndicator = this.GetResultIndicator(workspaceNode);

                


                var status = string.Empty;
                //if (workspaceNode.RepositoryStatus is not null)
                //{
                //    var stagedCount = workspaceNode.RepositoryStatus.Count(f => workspaceNode.Repository.Index.Any(i => i.Path == f.FilePath));
                //    var modifiedCount = workspaceNode.RepositoryStatus.Count() - stagedCount;

                //    var modified = modifiedCount == 0 ? string.Empty : $" [yellow]~{modifiedCount}[/]";
                //    var staged = stagedCount == 0 ? string.Empty : $" [teal]({stagedCount})[/]";

                //    status = $"{modified}{staged}";
                //}

                var treeNode = new TreeNode(new Markup($"{isActiveIndicator}{operationResultIndicator}[{color}]{workspaceNode.Name}[/]{branchIndicator}{status}"));

                parentNode.AddNode(treeNode);
                AddTreeNodes(workspaceNode, treeNode);
            }
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            this.tree = this.CreateTree(this.Workspace);
            return (tree as IRenderable).Measure(context, maxWidth);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            this.tree = this.CreateTree(this.Workspace);
            return (tree as IRenderable).Render(context, maxWidth);
        }

        private string GetResultIndicator(WorkspaceNode node)
        {
            string color;
            
            var lastResult = node.OperationResults.LastOrDefault();

            if (node.HasActiveProcess)
            {
                color = "yellow";
            }
            else if (lastResult is null)
            {
                return string.Empty;
            }
            else if (lastResult.ExitCode == 0)
            {
                color = node.HasRecentResult ? "lime" : "green4";
            }
            else
            {
                color = node.HasRecentResult ? "red" : "red3_1";
            }

            return $"[{color}]º [/]";
        }
    }
}
