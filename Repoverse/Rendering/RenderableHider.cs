using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse.Rendering
{
    public class RenderableHider : IRenderable
    {
        public RenderableHider(IRenderable renderable)
        {
            this.Renderable = renderable;
        }
        
        public IRenderable Renderable { get; }
        public bool IsHidden { get; set; }
        
        public Measurement Measure(RenderContext context, int maxWidth)
        {
            if (this.IsHidden)
            {
                return new Measurement(0, 0);
            }

            return this.Renderable.Measure(context, maxWidth);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            if (this.IsHidden)
            {
                return new[] { Segment.Empty };
            }

            return this.Renderable.Render(context, maxWidth);
        }
    }
}
