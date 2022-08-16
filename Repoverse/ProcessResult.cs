using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse
{
    public class ProcessResult
    {
        public string WorkingDirectoryPath { get; set; }
        public string Command { get; set; }
        public int ExitCode { get; set; }
        public string ProcessStandardOutput { get; set; }
        public string ProcessStandardError { get; set; }
    }
}
