using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repoverse
{
    internal static class Locks
    {
        public static object WorkspaceLock = new object();
    }
}
