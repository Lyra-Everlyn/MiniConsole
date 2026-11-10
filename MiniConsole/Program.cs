using System;
using System.Text;
using MiniConsole.Configuration;
using MiniConsole.Services;
using MiniConsole.Views;

namespace MiniConsole;

class Program
{
    static void Main(string[] args)
    {
        if (Environment.GetEnvironmentVariable("WT_SESSION") != null && (args.Length == 0 || args[0] != "--conhost"))
        {
            string? exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                System.Diagnostics.Process.Start("conhost.exe", $"\"{exePath}\" --conhost");
                return;
            }
        }

        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "MiniWorld Lossless JSON Engine V3";

        AppSettings settings = ConfigLoader.Load();
        string? logPath = LogPathLocator.FindTriggerLogFile(settings);

        if (string.IsNullOrEmpty(logPath))
        {
            ConsoleRenderer.PrintError("Could not locate 'TriggerScript.log'! Please verify your game installation or update 'TargetDataFolders' in appsettings.json.");
            Console.ReadLine();
            return;
        }

        ConsoleRenderer.PrintBanner(logPath, settings.RemoveLogTags, settings.PollIntervalMs);

        using var streamManager = new JsonStreamManager();
        var monitorService = new LogMonitorService(settings, streamManager);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => streamManager.CloseAllStreams();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            streamManager.CloseAllStreams();
            Environment.Exit(0);
        };

        monitorService.StartMonitoring(logPath);
    }
}