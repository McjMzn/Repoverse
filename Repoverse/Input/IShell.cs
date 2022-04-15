namespace Repoverse.Input
{
    public interface IShell : IProcessKeyPress
    {
        string Prompt { get; }
        string Help { get; }
    }
}
