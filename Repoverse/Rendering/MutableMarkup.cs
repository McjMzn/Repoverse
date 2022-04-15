using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Generic;

namespace Repoverse.Rendering
{
    public class MutableMarkup : IRenderable
    {
        private string markupText;
        private Markup markup;

        public MutableMarkup(string text)
        {
            this.markup = new Markup(text);
        }

        public string MarkupText
        {
            get { return this.markupText; }
            set
            {
                this.markupText = value;
                this.markup = new Markup(this.markupText ?? string.Empty);
            }
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            return (this.markup as IRenderable).Measure(context, maxWidth);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            return (this.markup as IRenderable).Render(context, maxWidth);
        }
    }
}
