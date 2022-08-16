using Repoverse.Input;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse.Rendering
{
    public class ShellRenderer : IRenderable
    {
        public IShell Shell { get; set; }

        public bool IsHidden { get; set; }
        
        public ShellRenderer(IShell shell)
        {
            this.Shell = shell;
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            return this.GetRenderable().Measure(context, maxWidth);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            return this.GetRenderable().Render(context, maxWidth);
        }
        
        private IRenderable GetRenderable()
        {
            if (this.IsHidden)
            {
                return new Markup(string.Empty);
            }
            
            switch (this.Shell)
            {
                case IAnsiShell ansiShell:
                    return new Markup($"[grey]{ansiShell.Help}[/]{Environment.NewLine}[yellow]{ansiShell.Prompt} [/]{ansiShell.AnsiInput}");

                case IShell shell:
                    return new Markup($"[grey]{shell.Help}[/][yellow]{shell.Prompt} [/]");

                default:
                    return new Markup(string.Empty);
            }
        }
    }
}
