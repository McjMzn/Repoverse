using Repoverse.Input;
using Repoverse.Rendering;
using Spectre.Console;
using System;
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

        public InteractiveLayoutRenderer(Repoverse repoverse)
        {
            this.workspaceRenderer = new WorkspaceRenderer(repoverse.Workspace);
            this.shellRenderer = new ShellRenderer(repoverse.ActiveShell);
            repoverse.ActiveShellChanged += (sender, shell) =>
            {
                this.shellRenderer.Shell = (sender as Repoverse).ActiveShell;
                if (this.shellRenderer.Shell is SimpleShell)
                {
                    // Allows scrolling up
                    //Thread.Sleep(20);
                    //this.Update();
                }
                else
                {
                    // this.resetEvent.Set();
                }
            };
            
            repoverse.OutputMessageProduced += (sender, message) => this.Write(message);
            
            var repoOutputTable =
                new Table()
                    .AddColumn(new TableColumn(workspaceRenderer))
                    .AddColumn(new TableColumn("<OUTPUT>"))
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
               //this.resetEvent.Set();
               //this.resetEvent.Reset();
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
        
        public void Write(string message)
        {
            this.mainLayoutHider.IsHidden = true;
            this.Update();

            this.StopLive();

            Console.WriteLine(message);

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
    }
    
}