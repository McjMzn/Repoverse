using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse
{
    public class ProcessResult
    {
        public string WorkingDirectoryPath { get; init; }
        public string Command { get; init; }
        public int ExitCode { get; init; }
        public string ProcessStandardOutput { get; init; }
        public string ProcessStandardError { get; init; }
    }
}
