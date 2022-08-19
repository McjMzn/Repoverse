using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse
{
    internal class SystemShell
    {
        private const string ProcessStartMagicString = "!@#START-OF-PROCESS#@!";
        private readonly string GetExitCodeCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "echo %errorlevel%" : "echo $?";
        private const string ProcessEndMagicString = "!@#END-OF-PROCESS#@!";

        private readonly Process shellProcess;
        private readonly StringBuilder standardOutput;
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        private int exitCode = 0;
        private string workingDirectory = null;
        private bool cacheWorkingDirectory = false;

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

        public string WorkingDirectory => this.cacheWorkingDirectory ? this.workingDirectory : this.ExecuteCommand("cd");

        public int ExitCode => this.exitCode;

        public string ExecuteCommand(string command)
        {
            this.cacheWorkingDirectory = false;

            this.shellProcess.StandardInput.Flush();
            this.shellProcess.StandardInput.WriteLine(ProcessStartMagicString);
            this.shellProcess.StandardInput.WriteLine($"{command}");
            this.shellProcess.StandardInput.WriteLine(GetExitCodeCommand);
            this.shellProcess.StandardInput.WriteLine(ProcessEndMagicString);
            this.resetEvent.WaitOne();
            this.resetEvent.Reset();

            var output = this.standardOutput.ToString().Trim();
            if (command == "cd")
            {
                this.workingDirectory = output;
                this.cacheWorkingDirectory = true;
            }

            return output;
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
