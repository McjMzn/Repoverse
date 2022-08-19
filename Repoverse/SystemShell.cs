using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse
{
    internal class SystemShell
    {
        private const string ProcessStartMagicString = "!@#START-OF-PROCESS#@!";
        private const string GetExitCodeCommand = "echo %errorlevel%";
        private const string ProcessEndMagicString = "!@#END-OF-PROCESS#@!";

        private readonly Process shellProcess;
        private readonly StringBuilder standardOutput;
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        private int exitCode = 0;

        public SystemShell(string workingDirectory)
        {
            this.standardOutput = new StringBuilder();

            var startInfo = new ProcessStartInfo("cmd")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
            };

            this.shellProcess = Process.Start(startInfo);
            this.StartStandardOutputReading();
            
        }

        public string WorkingDirectory => this.ExecuteCommand("cd");

        // TODO: Linux
        public int ExitCode => this.exitCode;

        public string ExecuteCommand(string command)
        {
            this.shellProcess.StandardInput.Flush();
            this.shellProcess.StandardInput.WriteLine(ProcessStartMagicString);
            this.shellProcess.StandardInput.WriteLine($"{command}");
            this.shellProcess.StandardInput.WriteLine(GetExitCodeCommand);
            this.shellProcess.StandardInput.WriteLine(ProcessEndMagicString);
            this.resetEvent.WaitOne();
            this.resetEvent.Reset();

            return this.standardOutput.ToString().Trim();
        }

        private Task StartStandardOutputReading()
        {
            return
                Task.Run(() =>
                {
                    while (!this.shellProcess.HasExited)
                    {
                        var oLine = this.shellProcess.StandardOutput.ReadLine();
                        if (oLine.Contains(ProcessStartMagicString))
                        {
                            this.standardOutput.Clear();

                            // Read command echo
                            var a = this.shellProcess.StandardOutput.ReadLine();
                            var b = this.shellProcess.StandardOutput.ReadLine();
                            continue;
                        }

                        if (oLine.Contains(GetExitCodeCommand))
                        {
                            var line = this.shellProcess.StandardOutput.ReadLine();
                            this.exitCode = int.Parse(line);
                            continue;
                        }

                        if (oLine.Contains(ProcessEndMagicString))
                        {
                            this.resetEvent.Set();
                            continue;
                        }

                        this.standardOutput.AppendLine(oLine);
                    }
                });
        }
    }
}
