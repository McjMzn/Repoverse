using Repoverse.Input;
using Repoverse.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse;

public class Program
{
    public static void Main(string[] args)
    {
        var workdir = @"C:\Users\mazan\Repos";

        var repoverse = new Repoverse(workdir);

        using (var renderer = new InteractiveLayoutRenderer(repoverse))
        {
            renderer.StartLive();
            while (true)
            {
                var key = Console.ReadKey(true);
                renderer.Update();
                repoverse.ProcessKeyPress(key);
            }
        }
    }

    public class InteractiveLayoutRenderer : IDisposable
    {
        private RenderableHider mainLayoutHider;
        private Table mainLayout;
        private WorkspaceRenderer workspaceRenderer;
        private ShellRenderer shellRenderer;

        private ManualResetEvent resetEvent = new ManualResetEvent(true);

        private Task liveTask;
        private bool running;

        private readonly Repoverse repoverse;

        public InteractiveLayoutRenderer(Repoverse repoverse)
        {
            this.repoverse = repoverse;
            this.workspaceRenderer = new WorkspaceRenderer(repoverse.Workspace, node => repoverse.SelectedNode == node);
            this.shellRenderer = new ShellRenderer(repoverse.ActiveShell);
            repoverse.ActiveShellChanged += (sender, shell) =>
            {
                this.shellRenderer.Shell = (sender as Repoverse).ActiveShell;
                if (this.shellRenderer.Shell is SimpleShell)
                {
                    // Allows scrolling up
                    Thread.Sleep(50);
                    this.Update();
                }
                else
                {
                    this.resetEvent.Set();
                }
            };

            repoverse.ProcessResultProvided += (sender, result) =>
            {
                var builder = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(result.ProcessStandardOutput))
                {
                    builder.AppendLine(result.ProcessStandardOutput);
                }

                if (!string.IsNullOrWhiteSpace(result.ProcessStandardError))
                {
                    builder.AppendLine($"[red]{result.ProcessStandardError}[/]");
                }

                var panel = new Panel(new Markup(builder.ToString())).Expand();
                var b = panel.Border.GetPart(BoxBorderPart.Top);
                panel.Header = new PanelHeader($"{b} [yellow]{result.WorkingDirectoryPath}>[/] [white]{result.Command}[/] {b}");

                this.Write(panel);
            };

            var repoOutputTable =
                new Table()
                    .AddColumn(new TableColumn(workspaceRenderer))
                    .Expand();

            this.mainLayout = new Table()
                .NoBorder()
                .HideHeaders()
                .Expand()
                .AddColumn(new TableColumn("").Padding(0, 0, 0, 0))
                .AddRow(repoOutputTable)
                .AddRow(new Panel(this.shellRenderer).Expand().NoBorder());

            this.mainLayoutHider = new RenderableHider(this.mainLayout);
        }

        public void Update()
        {
            if (this.shellRenderer.Shell is SimpleShell)
            {
                this.resetEvent.Set();
                this.resetEvent.Reset();
            }
        }

        public void StartLive()
        {
            if (liveTask is not null && !liveTask.IsCompleted)
            {
                return;
            }

            this.running = true;
            this.liveTask = Task.Run(() =>
            {
                AnsiConsole.Live(this.mainLayoutHider).Start(context =>
                {
                    while (this.running)
                    {
                        this.resetEvent.WaitOne();
                        context.Refresh();
                        Thread.Sleep(10);
                    }
                });
            });
        }

        public void Write(IRenderable renderable)
        {
            this.mainLayoutHider.IsHidden = true;
            this.Update();
            this.StopLive();

            this.MoveCursorUp();

            AnsiConsole.Write(renderable);
            // AnsiConsole.MarkupLine(message);

            this.mainLayoutHider.IsHidden = false;
            this.StartLive();
            Thread.Sleep(50);
            this.Update();
        }

        public void StopLive()
        {
            this.running = false;
            this.liveTask.Wait();
        }

        public void Dispose()
        {
            this.StopLive();
        }

        private void MoveCursorUp()
        {
            // Border
            Console.CursorTop--;
            Console.CursorTop--;

            // Legend
            Console.CursorTop--;

            // Nodes
            Action<WorkspaceNode> moveUp = null;
            moveUp = node =>
            {
                if (node.IsRepository || node.ContainRepositories)
                {
                    Console.CursorTop--;
                }

                foreach (var n in node.Nodes)
                {
                    moveUp(n);
                }
            };

            moveUp(this.repoverse.Workspace);
        }
    }

}