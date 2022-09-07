using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse.Rendering
{
    public enum ChildrenRenderingMode
    {
        All,
        AllIfNoRepositories,
        JustRepositories,
        None
    }

    public class WorkspaceRenderer : IRenderable
    {
        private readonly Predicate<WorkspaceNode> isSelected;
        private Tree tree;

        public WorkspaceNode Workspace { get; set; }

        public WorkspaceRenderer(WorkspaceNode node, Predicate<WorkspaceNode> isSelected)
        {
            this.Workspace = node;
            this.isSelected = isSelected;
        }

        private Tree CreateTree(WorkspaceNode workspace)
        {
            lock (Locks.WorkspaceLock)
            {
                var builder = new StringBuilder($"[{ColorScheme.Default.RootNode}]");
                if (workspace.IsSubdirectoryOfRepository)
                {
                    var repoPathSegments = workspace.Repository.Info.WorkingDirectory.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                    var workspacePathSegments = workspace.Path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

                    for (var i = 0; i < workspacePathSegments.Count(); i++)
                    {
                        if (i == repoPathSegments.Count() - 1)
                        {
                            builder.Append($"[{ColorScheme.Default.RepositoryBranch} underline]{workspacePathSegments[i]}[/]");
                        }
                        else
                        {
                            builder.Append(workspacePathSegments[i]);
                        }

                        builder.Append(Path.DirectorySeparatorChar);
                    }
                }

                builder.Append("[/]");

                var treeLabel = workspace.IsSubdirectoryOfRepository ? builder.ToString() : $"[{ColorScheme.Default.RootNode}]{workspace.Path}[/]";

                var tree = new Tree(new Markup(treeLabel));
                AddTreeNodes(workspace, tree,  workspace.ContainRepositories ? ChildrenRenderingMode.JustRepositories : ChildrenRenderingMode.AllIfNoRepositories);
                return tree;
            }
        }

        public void AddTreeNodes(WorkspaceNode workspace, IHasTreeNodes parentNode, ChildrenRenderingMode childrenRenderingMode)
        {
            if (childrenRenderingMode is ChildrenRenderingMode.None)
            {
                return;
            }

            foreach (var workspaceNode in workspace.Nodes)
            {
                if (childrenRenderingMode is ChildrenRenderingMode.JustRepositories && !workspaceNode.IsRepository && !workspaceNode.ContainRepositories)
                {
                    continue;
                }

                var nextMode =
                    childrenRenderingMode is ChildrenRenderingMode.AllIfNoRepositories ? ChildrenRenderingMode.None :
                    workspaceNode.IsRepository ? ChildrenRenderingMode.None :
                    workspaceNode.ContainRepositories ? ChildrenRenderingMode.JustRepositories :
                    ChildrenRenderingMode.None;

                var color = workspaceNode.IsRepository ? ColorScheme.Default.RepositoryNode : ColorScheme.Default.NonRepositoryNode;
                var branchIndicator = workspaceNode.IsRepository ? $"[{ColorScheme.Default.RepositoryBranch}] ({workspaceNode.Repository.Head.FriendlyName})[/]" : string.Empty;
                var isSelectedIndicator = this.isSelected(workspaceNode) ? $"> " : string.Empty;
                var isActiveIndicator =
                    !workspaceNode.IsRepository ? string.Empty :
                    workspaceNode.IsActive ? $"{isSelectedIndicator}[{ColorScheme.Default.ActiveRepository}]■ [/]" : $"{isSelectedIndicator}[{ColorScheme.Default.InactiveRepository}]■ [/]";

                var operationResultIndicator = this.GetResultIndicator(workspaceNode);

                var status = string.Empty;

                var treeNode = new TreeNode(new Markup($"{isActiveIndicator}{operationResultIndicator}[{color}]{workspaceNode.Name}[/]{branchIndicator}{status}"));


                parentNode.AddNode(treeNode);

                AddTreeNodes(workspaceNode, treeNode, nextMode);
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
                color = ColorScheme.Default.ActiveProcess;
            }
            else if (lastResult is null)
            {
                return string.Empty;
            }
            else if (lastResult.ExitCode == 0)
            {
                color = node.HasRecentResult ? ColorScheme.Default.RecentSuccessResult : ColorScheme.Default.SuccessResult;
            }
            else
            {
                color = node.HasRecentResult ? ColorScheme.Default.RecentErrorResult : ColorScheme.Default.ErrorResult;
            }

            return $"[{color}]º [/]";
        }
    }
}
