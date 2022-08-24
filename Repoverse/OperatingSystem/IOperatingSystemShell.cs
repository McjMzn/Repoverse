namespace Repoverse.OperatingSystem
{
    public interface IOperatingSystemShell
    {
        ShellCommandResult ExecuteCommand(string command);
    }
}
