using System;
using System.IO;
using System.Reflection;

namespace Repoverse;

public partial class Program
{
    public static void Main(string[] args)
    {
        var workdir = args.Length > 0 && Directory.Exists(args[0]) ? args[0] : Environment.CurrentDirectory;

        var repoverse = new Repoverse(workdir);

        var renderer = new InteractiveLayoutRenderer(repoverse);
        renderer.StartLive();

        var isRunning = true;
        Console.CancelKeyPress += (_, _) => isRunning = false;
        while (isRunning)
        {
            var key = Console.ReadKey(true);
            renderer.UpdateIfInControlShell();
            repoverse.ProcessKeyPress(key);
        }

        Console.WriteLine("Exiting Repoverse.");
        renderer.StopLive();
    }
}