namespace Repoverse.OperatingSystem;

public class OperatingSystemShellSettings
{
    public string ShellExecutablePath { get; init; }
    public string ShellExecutableArguments { get; init; }
    public string ExitCodeVariableName { get; init; }
    public string CommandsSequenceOperator { get; init; }

    public static OperatingSystemShellSettings Cmd { get; } = new()
    {
        ShellExecutablePath = "cmd",
        ShellExecutableArguments = "/c",
        CommandsSequenceOperator = "&&",
        ExitCodeVariableName = "%errorlevel%"
    };
    
    public static OperatingSystemShellSettings WslUbuntu2204 { get; } = new()
    {
        ShellExecutablePath = "ubuntu2204",
        ShellExecutableArguments = "run",
        CommandsSequenceOperator = ";",
        ExitCodeVariableName = "$?"
    };

    public static OperatingSystemShellSettings Bash { get; } = new()
    {
        ShellExecutablePath = "/bin/bash",
        ShellExecutableArguments = string.Empty,
        CommandsSequenceOperator = ";",
        ExitCodeVariableName = "$?"
    };
}