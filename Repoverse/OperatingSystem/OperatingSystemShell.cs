using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Repoverse.OperatingSystem;

public class OperatingSystemShell : IOperatingSystemShell
{
    private readonly OperatingSystemShellSettings settings;
    public OperatingSystemShell(string initialWorkingDirectory, OperatingSystemShellSettings settings)
    {
        this.settings = settings;
        if (!Directory.Exists(initialWorkingDirectory))
        {
            throw new DirectoryNotFoundException(initialWorkingDirectory);
        }
        
        this.WorkingDirectory = initialWorkingDirectory;
    }

    public string WorkingDirectory { get; private set; }
    
    public ShellCommandResult ExecuteCommand(string command)
    {
        return command.StartsWith("cd ") ? this.ExecuteCdCommand(command) : this.ExecuteRegularCommand(command);
    }

    private ShellCommandResult ExecuteRegularCommand(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = this.settings.ShellExecutablePath,
            Arguments = $"{this.settings.ShellExecutableArguments} \"{command} {settings.CommandsSequenceOperator} echo {this.settings.ExitCodeVariableName}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = this.WorkingDirectory
        };

        var output = new StringBuilder();
        var lines = new List<string>();
        DataReceivedEventHandler onOutputReceived = (sender, args) =>
        {
            if (args.Data is null)
            {
                return;
            }
            
            lines.Add(args.Data);
        };
        
        var process = Process.Start(processStartInfo);
        process.OutputDataReceived += onOutputReceived;
        process.ErrorDataReceived += onOutputReceived;
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();

        var exitCodeText = lines.Last().Trim();
        
        return new ShellCommandResult
        {
            Command = command,
            Output = string.Join(Environment.NewLine, lines.Take(lines.Count - 1)).Trim(),
            ExitCode = int.Parse(exitCodeText)
        };
    }

    private ShellCommandResult ExecuteCdCommand(string command)
    {
        var path = command.Substring("cd ".Length);
        var newPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(this.WorkingDirectory, path));

        var directoryExists = Directory.Exists(newPath);
        if (directoryExists)
        {
            this.WorkingDirectory = newPath;
            return new ShellCommandResult
            {
                Command = command,
                Output = $"Working directory changed to: \"{this.WorkingDirectory}\".",
                ExitCode = 0,
            };
        }
        
        return  new ShellCommandResult
        {
            Command = command,
            Output = $"Directory \"{newPath}\" does not exist.",
            ExitCode = 1,
        };
    }
    
}

