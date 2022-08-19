using Spectre.Console;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Repoverse;

public partial class Program
{
    public static void Main(string[] args)
    {
        try
        {
            PrintHeader();
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
        catch (Exception e)
        {

        }
    }

    private static void PrintHeader()
    {
        var version = typeof(Repoverse).Assembly.GetName().Version;

        var builder = new StringBuilder();
        builder.AppendLine("Repoverse");
        builder.AppendLine(version.ToString(3));
        builder.Append($"https://github.com/McjMzn/Repoverse");

        var content = new Markup(builder.ToString());
        content.Alignment = Justify.Center;

        var panel = new Panel(content).Expand();

        AnsiConsole.Write(panel);
    }
}