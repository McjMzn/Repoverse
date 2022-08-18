using System.Collections.Generic;

namespace Repoverse.Input
{
    public interface IAnsiShell : IShell
    {
        string RawInput { get; }
        string AnsiInput { get; }
        
        List<string> CommandHistory { get; }
    }
}
