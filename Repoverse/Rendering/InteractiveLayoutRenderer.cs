using Repoverse.Input;
using Repoverse.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse;

public class InteractiveLayoutRenderer
{
    private readonly Repoverse repoverse;

    private Table mainLayout;
    private RenderableHider mainLayoutHider;
    private WorkspaceRenderer workspaceRenderer;
    private ShellRenderer shellRenderer;

    private ManualResetEvent resetEvent = new ManualResetEvent(true);

    private Task liveTask;
    private bool running;

    public InteractiveLayoutRenderer(Repoverse repoverse)
    {
        this.repoverse = repoverse;
        this.workspaceRenderer = new WorkspaceRenderer(repoverse.Workspace, node => repoverse.SelectedNode == node);
        this.shellRenderer = new ShellRenderer(repoverse.ActiveShell);

        repoverse.ActiveShellChanged += OnActiveShellChanged;
        repoverse.ProcessResultProvided += OnProcessResultProvided;

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

    public bool UpdateIfInControlShell()
    {
        if (this.shellRenderer.Shell is SimpleShell and not AnsiShell)
        {
            this.resetEvent.Set();
            this.resetEvent.Reset();

            return true;
        }

        return false;
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
                    if (this.workspaceRenderer.Workspace.Path != this.repoverse.Workspace.Path)
                    {
                        this.workspaceRenderer.Workspace = this.repoverse.Workspace;
                        this.shellRenderer.Shell = this.repoverse.ActiveShell;
                    }

                    context.Refresh();
                    Thread.Sleep(10);
                }
            });
        });
    }

    public void Write(IRenderable renderable)
    {
        this.mainLayoutHider.IsHidden = true;
        this.UpdateIfInControlShell();
        this.StopLive();

        this.MoveCursorUp();

        AnsiConsole.Write(renderable);

        this.mainLayoutHider.IsHidden = false;
        this.StartLive();
        Thread.Sleep(50);
        this.UpdateIfInControlShell();
    }

    public void StopLive()
    {
        this.running = false;
        this.liveTask.Wait();
    }

    private void OnActiveShellChanged(object sender, IShell shell)
    {
        this.shellRenderer.Shell = (sender as Repoverse).ActiveShell;

        Thread.Sleep(50);
        if (!this.UpdateIfInControlShell())
        {
            this.resetEvent.Set();
        }
    }

    private void OnProcessResultProvided(object sender, ProcessResult result)
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
