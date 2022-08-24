using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse.OperatingSystem
{
    public class ShellCommandResult
    {
        public string Command { get; init; }
        public string Output { get; init; }
        public int ExitCode { get; init; }
    }
}
