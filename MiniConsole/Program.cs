using System;
using System.Text;
using MiniConsole.Configuration;
using MiniConsole.Services;
using MiniConsole.Views;

namespace MiniConsole;

class Program
{
    static async Task Main(string[] args)
    {
        if (WindowIconHelper.EnsureStandaloneWithCustomIcon(args))
        {
            return;
        }

        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "MiniConsole";

        await AutoUpdaterService.CheckAndApplyUpdateAsync();

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