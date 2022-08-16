using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse
{
    public class ProcessRunner
    {
        public static void Run(string commandWithArguments, IEnumerable<WorkspaceNode> nodes, bool parallel)
        {
            if (string.IsNullOrWhiteSpace(commandWithArguments))
            {
                return;
            }
            
            Parallel.ForEach(
                nodes,
                parallel ? new ParallelOptions() : new ParallelOptions { MaxDegreeOfParallelism = 1 },
                node => RunSingle(commandWithArguments, node)
            );
        }
        
        private static void RunSingle(string commandWithArguments, WorkspaceNode node)
        {
            node.HasRecentResult = false;
            if (!node.IsActive)
            {
                return;
            }

            // TODO: Change to support quotes
            var spaceIndex = commandWithArguments.IndexOf(' ');
            var command = spaceIndex > 0 ? commandWithArguments.Substring(0, spaceIndex) : commandWithArguments;
            var args = spaceIndex > 0 ? commandWithArguments.Substring(spaceIndex) : string.Empty;

            var startInfo = new ProcessStartInfo(command)
            {
                Arguments = args,
                WorkingDirectory = node.Path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var result = new ProcessResult
            {
                Command = commandWithArguments,
                WorkingDirectoryPath = node.Path,
            };

            try
            {
                using var process = Process.Start(startInfo);
                node.HasActiveProcess = true;
                process.WaitForExit();

                result.ProcessStandardOutput = process.StandardOutput.ReadToEnd().Replace("\t", "    ").Trim();
                result.ProcessStandardError = process.StandardError.ReadToEnd().Replace("\t", "    ").Trim();
                result.ExitCode = process.ExitCode;
            }
            catch (Exception e)
            {
                result.ExitCode = -1;
                result.ProcessStandardError = e.Message;
            }

            node.OperationResults.Add(result);
            node.HasActiveProcess = false;
            node.HasRecentResult = true;
        }
    }
}
