using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse.OperatingSystem
{
    internal class WindowsCmd : IOperatingSystemShell
    {
        const string ShellApplication = "cmd";

        private string workingDirectory;

        private Process process;

        private string command;
        private int exitCode;
        private bool stopRegistered = false;
        private ManualResetEvent manualReset = new ManualResetEvent(false);

        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private StringBuilder output = new StringBuilder();


        public WindowsCmd(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            var startInfo = new ProcessStartInfo(ShellApplication)
            {
                WorkingDirectory = this.workingDirectory,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
            };

            process = Process.Start(startInfo);

            BeginQueueProcessing();
        }

        public ShellCommandResult ExecuteCommand(string command)
        {
            this.command = command;
            process.StandardInput.Flush();
            process.StandardInput.WriteLine($"echo \"#START#\"");
            process.StandardInput.WriteLine(command);
            if (command.StartsWith("cd "))
            {
                var path = command.Substring("cd ".Length).Trim('"');
                var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(this.workingDirectory, path));
                if (Directory.Exists(fullPath))
                {
                    this.workingDirectory = fullPath;
                }
            }

            process.StandardInput.WriteLine($"echo %errorlevel%");
            process.StandardInput.WriteLine($"echo \"#STOP#\"");
            manualReset.WaitOne();

            return
                new ShellCommandResult
                {
                    Command = command,
                    Output = output.ToString().Trim(),
                    ExitCode = this.exitCode,
                };
        }

        private Task BeginQueueingFromStream(StreamReader stream)
        {
            return
                Task.Run(() =>
                {
                    while (!process.HasExited)
                    {
                        var line = stream.ReadLine();
                        queue.Enqueue(line);
                    }
                });
        }

        private string Dequeue()
        {
            string text = null;
            while (!queue.TryDequeue(out text))
            {
                Thread.Sleep(10);
            }

            return text;
        }

        private void BeginQueueProcessing()
        {
            BeginQueueingFromStream(process.StandardOutput);
            BeginQueueingFromStream(process.StandardError);

            Task.Run(() =>
            {
                while (!process.HasExited)
                {
                    // No active command.
                    if (command is null)
                    {
                        continue;
                    }

                    // Queue emptied.
                    if (queue.Count == 0)
                    {
                        if (stopRegistered)
                        {
                            manualReset.Set();
                            stopRegistered = false;
                            manualReset.Reset();
                        }

                        Thread.Sleep(50);
                        continue;
                    }

                    var text = Dequeue();

                    // Command process starting.
                    if (text.Contains("#START#"))
                    {
                        output.Clear();
                        continue;
                    }

                    // Echo of working directory and command name.
                    if (text.Contains(command) && text.Contains(workingDirectory, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    // Process finished.
                    if (text.Contains("#STOP#"))
                    {
                        stopRegistered = true;
                        continue;
                    }

                    // Check exit code.
                    if (text.Contains("echo %errorlevel%"))
                    {
                        text = Dequeue();

                        if (int.TryParse(text, out int exitCode))
                        {
                            this.exitCode = exitCode;
                            continue;
                        }
                    }

                    output.AppendLine(text);
                }
            });
        }
    }
}
